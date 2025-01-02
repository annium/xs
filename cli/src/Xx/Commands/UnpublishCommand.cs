using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Server.Client;
using Server.Client.Clients;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;
using Xx.Cli.Core.Tasks;
using Xx.Cli.Core.Tools;
using Xx.Tools;
using Version = Xx.Cli.Core.Models.Version;

namespace Xx.Commands;

internal class UnpublishCommand
    : AsyncCommand<UnpublishCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "unpublish";
    public static string Description => "Unpublish package from registry.";
    public ILogger Logger { get; }
    private readonly IConfigurationManager _configurationManager;
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ProjectsRunner _runner;
    private readonly ServerClientFactory _serverClientFactory;

    public UnpublishCommand(
        IConfigurationManager configurationManager,
        DiscoverProjectsTask discoverTask,
        ProjectsRunner runner,
        ServerClientFactory serverClientFactory,
        ILogger logger
    )
    {
        _configurationManager = configurationManager;
        _discoverTask = discoverTask;
        _runner = runner;
        _serverClientFactory = serverClientFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(
        UnpublishCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var configuration = _configurationManager.Load(discoverCfg.Root);

        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterMask(cfg.Mask).OfType<IPublishableProject>().ToArray();

        if (projects.Length == 0)
        {
            this.Info("No projects found unpublish.");
            return;
        }

        var clients = new Dictionary<ProjectType, ServerClient>();
        foreach (var type in projects.Select(p => p.Type).Distinct())
        {
            if (configuration.Servers.TryGetValue(type, out var registryUri))
                clients[type] = _serverClientFactory.Create(registryUri);
            else
                throw new InvalidOperationException($"Registry doesn't support project type '{type}'.");
        }

        this.Debug("Unpublish {projectsLength} projects.", projects.Length);
        await _runner.RunAsync(
            projects,
            (project, _) =>
                clients[project.Type].DeletePackageAsync(configuration.Token, project.Name, cfg.Version.ToString()),
            new ProjectsRunner.Config(cfg.Parallelism, cfg.Deep),
            ct
        );
    }
}

internal class UnpublishCommandConfiguration
{
    [Position(1)]
    [Help("Project mask.")]
    public string Mask { get; set; } = string.Empty;

    [Position(2)]
    [Help("Version to unpublish.")]
    public Version Version { get; set; } = Version.Empty;

    [Option("d")]
    [Help("Unpublish dependencies.")]
    public bool Deep { get; set; }

    [Option("p")]
    [Help("Degree of parallelism. Default - all available tasks are run in parallel")]
    public int Parallelism { get; set; }
}
