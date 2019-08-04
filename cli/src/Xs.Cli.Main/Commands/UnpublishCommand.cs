using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;
using Xs.RegistryClient.Server;

namespace Xs.Cli.Main.Commands
{
    internal class UnpublishCommand : AsyncCommand<UnpublishCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "unpublish";
        public override string Description { get; } = "Unpublish package from registry.";
        private readonly IConfigurationManager configurationManager;
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ProjectsRunner runner;
        private readonly ServerClientFactory serverClientFactory;
        private readonly ILogger<UnpublishCommand> logger;

        public UnpublishCommand(
            IConfigurationManager configurationManager,
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ServerClientFactory serverClientFactory,
            ILogger<UnpublishCommand> logger
        )
        {
            this.configurationManager = configurationManager;
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.serverClientFactory = serverClientFactory;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            UnpublishCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var configuration = await configurationManager.LoadAsync(discoverCfg.Root);
            if (configuration == null)
                throw new InvalidOperationException("Registry is not tracked. Track it to unpublish.");

            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .OfType<IPublishableProject>()
                .ToArray();

            if (projects.Length == 0)
            {
                logger.Info($"No projects found unpublish.");
                return;
            }

            var clients = new Dictionary<ProjectType, ServerClient>();
            foreach (var type in projects.Select(p => p.Type).Distinct())
            {
                if (configuration.Servers.ContainsKey(type))
                    clients[type] = serverClientFactory.Create(configuration.Servers[type]);
                else
                    throw new InvalidOperationException($"Registry doesn't support project type '{type}'.");
            }

            logger.Debug($"Unpublish {projects.Length} projects.");
            await runner.RunAsync(
                projects,
                (project, tkn) => clients[project.Type].DeletePackageAsync(configuration.Token, project.Name, cfg.Version.ToString()),
                token
            );
        }
    }

    internal class UnpublishCommandConfiguration
    {
        [Position(1)]
        [Help("Project mask.")]
        public string Mask { get; set; }

        [Position(2)]
        [Help("Version to unpublish.")]
        public Core.Models.Version Version { get; set; }
    }
}