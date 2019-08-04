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

namespace Xs.Cli.Main.Commands
{
    internal class InstallCommand : AsyncCommand<InstallCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "install";

        public override string Description { get; } = "Install projects' dependencies.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly ProjectsRunner runner;

        private readonly ILogger<InstallCommand> logger;

        public InstallCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<InstallCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            InstallCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var force = cfg.Force;

            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();

            if (force)
            {
                logger.Debug($"Clear {projects.Length} projects cache.");
                await runner.RunAsync(
                    projects.OfType<ICachingProject>(),
                    (project, tkn) => project.ClearCacheAsync(tkn),
                    token);
            }

            logger.Debug($"Install {projects.Length} projects.");
            await runner.RunAsync(
                projects.OfType<IInstallableProject>(),
                (project, tkn) => project.InstallAsync(force, tkn),
                token);
        }
    }

    internal class InstallCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option("f", isRequired : false)]
        [Help("Force install.")]
        public bool Force { get; set; } = false;
    }
}