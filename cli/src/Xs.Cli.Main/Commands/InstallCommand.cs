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
    internal class InstallCommand : AsyncCommand<InstallCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "install";

        public override string Description { get; } = "Install projects' dependencies.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly ProjectsRunner runner;

        private readonly ILogger logger;

        public InstallCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            ProjectsRunner runner,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            InstallCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var force = cfg.Force;

            var projects = filterTask.Run(discoverTask.Run(cwdCfg.Cwd), cfg.Mask).ToArray();

            if (force)
            {
                logger.LogDebug($"Clear {projects.Length} projects cache.");
                await runner.RunAsync(
                    projects.OfType<ICachingProject>(),
                    (project, tkn) => project.ClearCacheAsync(tkn),
                    token);
            }

            logger.LogDebug($"Install {projects.Length} projects.");
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

        [Option("f", isRequired : false)]
        [Help("Force install.")]
        public bool Force { get; set; } = false;
    }
}