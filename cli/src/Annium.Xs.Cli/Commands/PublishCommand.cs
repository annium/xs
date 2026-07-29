using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Cli.Tools;
using Version = Annium.Xs.Cli.Core.Models.Version;

namespace Annium.Xs.Cli.Commands;

internal class PublishCommand
    : AsyncCommand<PublishCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "publish";
    public static string Description => "Publish packages to registry.";
    public ILogger Logger { get; }
    private readonly IConfigurationManager _configurationManager;
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ProjectsRunner _runner;

    public PublishCommand(
        IConfigurationManager configurationManager,
        DiscoverProjectsTask discoverTask,
        ProjectsRunner runner,
        ILogger logger
    )
    {
        _configurationManager = configurationManager;
        _discoverTask = discoverTask;
        _runner = runner;
        Logger = logger;
    }

    public override async Task HandleAsync(
        PublishCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var configuration = await _configurationManager.LoadAsync(discoverCfg.Root, ct);

        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterMask(cfg.Mask).OfType<IPublishableProject>().ToArray();

        if (projects.Length == 0)
        {
            this.Info("No projects found publish.");
            return;
        }

        foreach (var project in projects)
            if (!configuration.Servers.ContainsKey(project.Type))
                throw new InvalidOperationException($"Registry doesn't support project type '{project.Type}'.");

        this.Debug("Publish {projectsLength} projects.", projects.Length);
        await _runner.RunAsync(
            projects,
            (project, tkn) =>
                project.PublishAsync(configuration.Servers[project.Type], configuration.Token, cfg.Version, tkn),
            new ProjectsRunner.Config(cfg.Parallelism, cfg.Deep),
            ct
        );
    }
}

internal class PublishCommandConfiguration
{
    [Position(1)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = string.Empty;

    [Position(2)]
    [Help("Version to publish.")]
    public Version Version { get; set; } = Version.Empty;

    [Option("d")]
    [Help("Publish dependencies.")]
    public bool Deep { get; set; }

    [Option("p")]
    [Help("Degree of parallelism. Default - all available tasks are run in parallel")]
    public int Parallelism { get; set; }
}
