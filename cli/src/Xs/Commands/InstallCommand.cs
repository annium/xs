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
using Xs.Tools;

namespace Xs.Commands
{
    internal class InstallCommand : AsyncCommand<InstallCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "install";
        public override string Description { get; } = "Install projects' dependencies.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ProjectsRunner _runner;
        private readonly ILogger<InstallCommand> _logger;

        public InstallCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<InstallCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _runner = runner;
            _logger = logger;
        }

        public override async Task HandleAsync(
            InstallCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            var force = cfg.Force;

            var projects = _discoverTask.RunAsync(discoverCfg).Await()
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();

            if (force)
            {
                _logger.Debug($"Clear {projects.Length} projects cache.");
                await _runner.RunAsync(
                    projects.OfType<ICachingProject>(),
                    (project, tkn) => project.ClearCacheAsync(tkn),
                    cfg.Deep,
                    ct
                );
            }

            _logger.Debug($"Install {projects.Length} projects.");
            await _runner.RunAsync(
                projects.OfType<IInstallableProject>(),
                (project, tkn) => project.InstallAsync(force, tkn),
                cfg.Deep,
                ct
            );
        }
    }

    internal class InstallCommandConfiguration
    {
        [Position(1, isRequired: false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired: false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; } = ProjectType.None;

        [Option("f", isRequired: false)]
        [Help("Force install.")]
        public bool Force { get; set; }

        [Option("d")]
        [Help("Install dependencies.")]
        public bool Deep { get; set; }
    }
}