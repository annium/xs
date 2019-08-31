using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Tasks;
using Xs.Tools;

namespace Xs.Commands
{
    internal class BuildCommand : AsyncCommand<BuildCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "build";
        public override string Description { get; } = "Build projects.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ProjectsRunner runner;
        private readonly ILogger<BuildCommand> logger;

        public BuildCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<BuildCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            BuildCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .OfType<IBuildableProject>()
                .ToArray();
            logger.Debug($"Build {projects.Length} projects.");
            await runner.RunAsync(projects, (project, tkn) => project.BuildAsync(cfg.Env, tkn), cfg.Deep, token);
        }
    }

    internal class BuildCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option]
        [Help("Environment.")]
        public Env Env { get; set; } = Env.Development;

        [Option("d")]
        [Help("Build dependencies.")]
        public bool Deep { get; set; }
    }
}