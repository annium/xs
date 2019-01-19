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
    internal class CleanCommand : AsyncCommand<CwdCommandConfiguration>
    {
        public override string Id { get; } = "clean";

        public override string Description { get; } = "clean projects";

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
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = (await discoverTask.RunAsync(cwdCfg.Cwd))
                .OfType<ICleanableProject>()
                .ToArray();
            logger.LogDebug($"Cleaning {projects.Length} projects");
            await runner.RunAsync(projects, (project, tkn) => project.CleanAsync(tkn), token);
        }
    }
}