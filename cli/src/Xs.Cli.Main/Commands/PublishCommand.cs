using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands
{
    internal class PublishCommand : AsyncCommand<PublishCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "publish";
        public override string Description { get; } = "Publish packages to registry.";
        private readonly IConfigurationManager configurationManager;
        private readonly DiscoverProjectsTask discoverTask;
        private readonly FilterProjectsTask filterTask;
        private readonly ProjectsRunner runner;
        private readonly ILogger logger;

        public PublishCommand(
            IConfigurationManager configurationManager,
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            ProjectsRunner runner,
            ILogger logger
        )
        {
            this.configurationManager = configurationManager;
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            PublishCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var registry = (await configurationManager.Load(cwdCfg.Cwd)).Registries
                .FirstOrDefault(e => e.Name.ToLowerInvariant() == cfg.Registry.ToLowerInvariant());
            if (registry == null)
                throw new InvalidOperationException($"Registry {cfg.Registry} is not tracked. Track it to manipulate permissions.");

            var projects = filterTask.Run(await discoverTask.RunAsync(cwdCfg.Cwd), cfg.Mask)
                .OfType<IPublishableProject>()
                .ToArray();

            if (projects.Length == 0)
            {
                logger.LogInfo($"No projects found publish.");
                return;
            }

            foreach (var project in projects)
                if (!registry.Servers.ContainsKey(project.Type))
                    throw new InvalidOperationException($"Registry {registry} doesn't support project type '{project.Type}'.");

            logger.LogDebug($"Publish {projects.Length} projects.");
            await runner.RunAsync(
                projects,
                (project, tkn) => project.PublishAsync(registry.Servers[project.Type], registry.Token, cfg.Version, tkn),
                token
            );
        }
    }

    internal class PublishCommandConfiguration
    {
        [Position(1)]
        [Help("Registry.")]
        public string Registry { get; set; }

        [Position(2)]
        [Help("Projects mask.")]
        public string Mask { get; set; }

        [Position(3)]
        [Help("Version to publish.")]
        public Core.Models.Version Version { get; set; }
    }
}