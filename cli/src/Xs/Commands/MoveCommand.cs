using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Threading.Tasks;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands;

internal class MoveCommand : Command<MoveCommandConfiguration, DiscoverConfiguration>, ILogSubject<MoveCommand>
{
    public override string Id => "move";
    public override string Description => "Move project to different location.";
    public ILogger<MoveCommand> Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;

    public MoveCommand(
        DiscoverProjectsTask discoverTask,
        ILogger<MoveCommand> logger
    )
    {
        _discoverTask = discoverTask;
        Logger = logger;
    }

    public override void Handle(
        MoveCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        if (!cfg.IsMove && !cfg.IsRename)
        {
            this.Log().Info("Specify at least new project name or new project directory");
            return;
        }

        var projects = _discoverTask.RunAsync(discoverCfg).Await().ToArray();
        var targets = projects.FilterMask(cfg.Filter).ToArray();
        if (targets.Length == 0)
            throw new InvalidOperationException($"No projects matched filter {cfg.Filter}.");

        if (cfg.IsRename)
        {
            if (targets.Length > 1)
                throw new InvalidOperationException($"Filter {cfg.Filter} has ambiguous match between {targets.Length} projects: {Environment.NewLine}{string.Join<IProject>(Environment.NewLine, targets)}.");

            var project = targets.Single();
            if (cfg.IsMove)
                Move(project, cfg.Directory!);
            Rename(project, cfg.Name!);
            Save(projects, project);
        }
        else if (cfg.IsMove)
        {
            foreach (var target in targets)
            {
                Move(target, cfg.Directory!);
                Save(projects, target);
            }
        }
    }

    private void Move(IProject project, string directory)
    {
        var target = Path.GetFullPath(Path.Combine(directory, Path.GetFileName(project.Directory)));
        this.Log().Debug($"Move {project.Directory} -> {target}");
        project.SetDirectory(target);
    }

    private void Rename(IProject project, string name)
    {
        this.Log().Debug($"Rename {project.Name} -> {name}");
        project.SetName(name);
    }

    private void Save(IReadOnlyCollection<IProject> projects, IProject project)
    {
        project.Save();
        var dependants = projects.Where(p => p.Projects.Any(d => d.Value == project)).ToArray();
        foreach (var dependant in dependants)
            dependant.Save();
    }
}

internal class MoveCommandConfiguration
{
    [Position(1)]
    [Help("Project name.")]
    public string Filter { get; set; } = string.Empty;

    [Option("n")]
    [Help("New project name.")]
    public string? Name { get; set; }

    [Option("d")]
    [Help("New project parent directory.")]
    public string? Directory { get; set; }

    public bool IsMove => !string.IsNullOrWhiteSpace(Directory);

    public bool IsRename => !string.IsNullOrWhiteSpace(Name);
}