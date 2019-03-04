using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands
{
    internal class CleanCommand : AsyncCommand<CleanCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "clean";

        public override string Description { get; } = "Clean projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly ProjectsRunner runner;

        private readonly ILogger logger;

        public CleanCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            CleanCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(cwdCfg.Cwd)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .OfType<ICleanableProject>()
                .ToArray();

            logger.Debug($"Clean {projects.Length} projects.");
            await runner.RunAsync(projects, (project, tkn) => project.CleanAsync(tkn), token);
        }
    }

    internal class CleanCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }
    }
}