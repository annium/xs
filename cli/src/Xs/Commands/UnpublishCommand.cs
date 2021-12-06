using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.RegistryClient.Server;
using Xs.Tools;

namespace Xs.Commands;

internal class UnpublishCommand : AsyncCommand<UnpublishCommandConfiguration, DiscoverConfiguration>, ILogSubject
{
    public override string Id => "unpublish";
    public override string Description => "Unpublish package from registry.";
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
        ILogger<UnpublishCommand> logger
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
        if (configuration == null)
            throw new InvalidOperationException("Registry is not tracked. Track it to unpublish.");

        var projects = _discoverTask.RunAsync(discoverCfg).Await()
            .FilterMask(cfg.Mask)
            .OfType<IPublishableProject>()
            .ToArray();

        if (projects.Length == 0)
        {
            this.Log().Info($"No projects found unpublish.");
            return;
        }

        var clients = new Dictionary<ProjectType, ServerClient>();
        foreach (var type in projects.Select(p => p.Type).Distinct())
        {
            if (configuration.Servers.ContainsKey(type))
                clients[type] = _serverClientFactory.Create(configuration.Servers[type]);
            else
                throw new InvalidOperationException($"Registry doesn't support project type '{type}'.");
        }

        this.Log().Debug($"Unpublish {projects.Length} projects.");
        await _runner.RunAsync(
            projects,
            (project, tkn) => clients[project.Type].DeletePackageAsync(configuration.Token, project.Name, cfg.Version.ToString()),
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
    public Cli.Core.Models.Version Version { get; set; } = Cli.Core.Models.Version.Empty;

    [Option("d")]
    [Help("Unpublish dependencies.")]
    public bool Deep { get; set; }

    [Option("p")]
    [Help("Degree of parallelism. Default - all available tasks are run in parallel")]
    public int Parallelism { get; set; }
}