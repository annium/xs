using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.RegistryClient.Server;
using Xs.Tools;

namespace Xs.Commands
{
    internal class UnpublishCommand : AsyncCommand<UnpublishCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "unpublish";
        public override string Description { get; } = "Unpublish package from registry.";
        private readonly IConfigurationManager _configurationManager;
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ProjectsRunner _runner;
        private readonly ServerClientFactory _serverClientFactory;
        private readonly ILogger<UnpublishCommand> _logger;

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
            _logger = logger;
        }

        public override async Task HandleAsync(
            UnpublishCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
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
                _logger.Info($"No projects found unpublish.");
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

            _logger.Debug($"Unpublish {projects.Length} projects.");
            await _runner.RunAsync(
                projects,
                (project, tkn) => clients[project.Type].DeletePackageAsync(configuration.Token, project.Name, cfg.Version.ToString()),
                cfg.Deep,
                token
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
    }
}