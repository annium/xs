using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Tasks;
using Xs.Tools;

namespace Xs.Commands
{
    internal class PublishCommand : AsyncCommand<PublishCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "publish";
        public override string Description { get; } = "Publish packages to registry.";
        private readonly IConfigurationManager configurationManager;
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ProjectsRunner runner;
        private readonly ILogger<PublishCommand> logger;

        public PublishCommand(
            IConfigurationManager configurationManager,
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<PublishCommand> logger
        )
        {
            this.configurationManager = configurationManager;
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            PublishCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var configuration = await configurationManager.LoadAsync(discoverCfg.Root);
            if (configuration == null)
                throw new InvalidOperationException("Registry is not tracked. Track it to publish.");

            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .OfType<IPublishableProject>()
                .ToArray();

            if (projects.Length == 0)
            {
                logger.Info($"No projects found publish.");
                return;
            }

            foreach (var project in projects)
                if (!configuration.Servers.ContainsKey(project.Type))
                    throw new InvalidOperationException($"Registry doesn't support project type '{project.Type}'.");

            logger.Debug($"Publish {projects.Length} projects.");
            await runner.RunAsync(
                projects,
                (project, tkn) => project.PublishAsync(configuration.Servers[project.Type], configuration.Token, cfg.Version, tkn),
                cfg.Deep,
                token
            );
        }
    }

    internal class PublishCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; }

        [Position(2)]
        [Help("Version to publish.")]
        public Cli.Core.Models.Version Version { get; set; }

        [Option("d")]
        [Help("Publish dependencies.")]
        public bool Deep { get; set; }
    }
}