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
    internal class CleanCommand : AsyncCommand<CleanCommandConfiguration, DiscoverConfiguration>, ILogSubject
    {
        public override string Id => "clean";
        public override string Description => "Clean projects.";
        public ILogger Logger { get; }
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ProjectsRunner _runner;

        public CleanCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<CleanCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _runner = runner;
            Logger = logger;
        }

        public override async Task HandleAsync(
            CleanCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            var projects = _discoverTask.RunAsync(discoverCfg).Await()
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .OfType<ICleanableProject>()
                .ToArray();

            this.Debug($"Clean {projects.Length} projects.");
            await _runner.RunAsync(projects, (project, tkn) => project.CleanAsync(cfg.Force, tkn), cfg.Deep, ct);
        }
    }

    internal class CleanCommandConfiguration
    {
        [Position(1, isRequired: false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired: false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; } = ProjectType.None;

        [Option("d")]
        [Help("Clean dependencies.")]
        public bool Deep { get; set; }

        [Option("f", isRequired: false)]
        [Help("Force clean.")]
        public bool Force { get; set; }
    }
}