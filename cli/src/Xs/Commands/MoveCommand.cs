using System;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands
{
    internal class MoveCommand : Command<MoveCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "move";
        public override string Description { get; } = "Move project to different location.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ILogger<MoveCommand> logger;

        public MoveCommand(
            DiscoverProjectsTask discoverTask,
            ILogger<MoveCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override void Handle(
            MoveCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var currentName = cfg.CurrentName;
            var name = cfg.Name;
            var directory = cfg.Directory;

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(directory))
            {
                logger.Info("Specify at least new project name or new project directory");
                return;
            }

            var projects = discoverTask.Run(discoverCfg).ToArray();
            var currentNameLower = currentName.ToLowerInvariant();
            var targets = projects.Where(p => p.Name.ToLowerInvariant().Contains(currentNameLower)).ToList();
            if (targets.Count == 0)
                throw new InvalidOperationException($"Project {currentName} not found.");
            if (targets.Count > 1)
                throw new InvalidOperationException($"Project {currentName} matches {targets.Count} projects: {Environment.NewLine}{string.Join(Environment.NewLine, targets)}.");
            var project = targets.Single();
            var dependants = projects.Where(p => p.Projects.Any(d => d.Value == project)).ToArray();

            // rename
            if (!string.IsNullOrWhiteSpace(name))
            {
                logger.Debug($"Rename {currentName} -> {name}");
                project.SetName(name);
            }

            // move
            if (!string.IsNullOrWhiteSpace(directory))
            {
                var target = Path.GetFullPath(Path.Combine(directory, Path.GetFileName(project.Directory)));
                logger.Debug($"Move {project.Directory} -> {target}");
                project.SetDirectory(target);
            }

            // save changes
            project.Save();
            foreach (var dependant in dependants)
                dependant.Save();
        }
    }

    internal class MoveCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string CurrentName { get; set; } = string.Empty;

        [Option("name")]
        [Help("New project name.")]
        public string? Name { get; set; }

        [Option("directory")]
        [Help("New project parent directory.")]
        public string? Directory { get; set; }
    }
}