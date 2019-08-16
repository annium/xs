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
    internal class TestCommand : AsyncCommand<TestCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "test";
        public override string Description { get; } = "Test projects.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ProjectsRunner runner;
        private readonly ILogger<TestCommand> logger;

        public TestCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<TestCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            TestCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .OfType<ITestableProject>()
                .ToArray();

            logger.Debug($"Test {projects.Length} projects.");
            await runner.RunAsync(projects, (project, tkn) => project.TestAsync(cfg.Env, cfg.TestFilter, tkn), token);
        }
    }

    internal class TestCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option("tf", isRequired : false)]
        [Help("Tests filter.")]
        public string TestFilter { get; set; } = string.Empty;

        [Option]
        [Help("Environment.")]
        public Env Env { get; set; } = Env.Development;
    }
}