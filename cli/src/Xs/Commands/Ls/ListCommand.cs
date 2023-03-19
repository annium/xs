using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Threading.Tasks;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands.Ls;

internal class ListCommand : Command<ListCommandConfiguration, DiscoverConfiguration>
{
    public override string Id => "";
    public override string Description => "List projects.";
    private readonly DiscoverProjectsTask _discoverTask;

    public ListCommand(
        DiscoverProjectsTask discoverTask
    )
    {
        _discoverTask = discoverTask;
    }

    public override void Handle(
        ListCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var projects = _discoverTask.RunAsync(discoverCfg).Await()
            .FilterMask(cfg.Mask)
            .FilterType(cfg.Type)
            .ToArray();

        foreach (var project in SelectProjects(projects, cfg))
            LogProject(project, cfg.Path, cfg.Attributes);
    }

    private IReadOnlyCollection<IProject> SelectProjects(
        IReadOnlyCollection<IProject> projects,
        ListCommandConfiguration cfg
    )
    {
        // without filters - plain list
        if (!cfg.Publishable && !cfg.Testable)
            return projects;

        var filtered = new List<IProject>();
        if (cfg.Publishable)
            filtered.AddRange(projects.OfType<IPublishableProject>());
        if (cfg.Testable)
            filtered.AddRange(projects.OfType<ITestableProject>());

        return cfg.Not ? projects.Except(filtered).ToArray() : filtered;
    }

    private void LogProject(IProject project, bool writePath, bool writeAttributes)
    {
        var sb = new StringBuilder();

        if (writePath)
            sb.Append(project.File);
        else if (writeAttributes)
        {
            sb.Append(project.Name);
            if (project is IPublishableProject)
                sb.Append(" [Publish]");

            if (project is ITestableProject)
                sb.Append(" [Test]");
        }
        else
            sb.Append(project.Name);

        Console.WriteLine(sb.ToString());
    }
}

internal class ListCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option]
    [Help("Show path instead of name.")]
    public bool Path { get; set; } = false;

    [Option]
    [Help("Invert selection.")]
    public bool Not { get; set; } = false;

    [Option("pub")]
    [Help("Show publishable projects.")]
    public bool Publishable { get; set; } = false;

    [Option("test")]
    [Help("Show publishable projects.")]
    public bool Testable { get; set; } = false;

    [Option("a")]
    [Help("Show project attributes.")]
    public bool Attributes { get; set; } = false;
}