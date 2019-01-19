using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands
{
    internal class BuildCommand : AsyncCommand<BuildCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "build";

        public override string Description { get; } = "build projects";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly ProjectsRunner runner;

        private readonly ILogger logger;

        public BuildCommand(
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
            BuildCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = filterTask.Run(await discoverTask.RunAsync(cwdCfg.Cwd), cfg.Mask)
                .OfType<IBuildableProject>()
                .ToArray();
            logger.LogDebug($"Building {projects.Length} projects");
            await runner.RunAsync(projects, (project, tkn) => project.BuildAsync(cfg.Env, tkn), token);
        }
    }

    internal class BuildCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask")]
        public string Mask { get; set; } = "*";

        [Position(2, isRequired : false)]
        [Help("Environment")]
        public Env Env { get; set; } = Env.Development;
    }
}