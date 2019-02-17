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
    internal class UnpublishCommand : AsyncCommand<UnpublishCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "unpublish";
        public override string Description { get; } = "Unpublish package from registry.";
        private readonly IConfigurationManager configurationManager;
        private readonly DiscoverProjectsTask discoverTask;
        private readonly FilterProjectsTask filterTask;
        private readonly ProjectsRunner runner;
        private readonly ILogger logger;

        public UnpublishCommand(
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
            UnpublishCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var configuration = await configurationManager.Load(cwdCfg.Cwd);
            if (configuration == null)
                throw new InvalidOperationException("Registry is not tracked. Track it to unpublish.");

            var projects = filterTask.Run(await discoverTask.RunAsync(cwdCfg.Cwd), cfg.Mask)
                .OfType<IPublishableProject>()
                .ToArray();

            if (projects.Length == 0)
            {
                logger.LogInfo($"No projects found unpublish.");
                return;
            }

            foreach (var project in projects)
                if (!configuration.Servers.ContainsKey(project.Type))
                    throw new InvalidOperationException($"Registry doesn't support project type '{project.Type}'.");

            logger.LogDebug($"Unpublish {projects.Length} projects.");
            await runner.RunAsync(
                projects,
                (project, tkn) => project.UnpublishAsync(configuration.Servers[project.Type], configuration.Token, cfg.Version, tkn),
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