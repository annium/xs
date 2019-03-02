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
    internal class TestCommand : AsyncCommand<TestCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "test";

        public override string Description { get; } = "Test projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly ProjectsRunner runner;

        private readonly ILogger logger;

        public TestCommand(
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
            TestCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(cwdCfg.Cwd)
                .FilterMask(cfg.Mask)
                .OfType<ITestableProject>()
                .ToArray();
            logger.LogDebug($"Test {projects.Length} projects.");
            await runner.RunAsync(projects, (project, tkn) => project.TestAsync(cfg.Env, tkn), token);
        }
    }

    internal class TestCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Environment.")]
        public Env Env { get; set; } = Env.Development;
    }
}