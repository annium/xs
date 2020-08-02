using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands
{
    internal class TestCommand : AsyncCommand<TestCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "test";
        public override string Description { get; } = "Test projects.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ProjectsRunner _runner;
        private readonly ILogger<TestCommand> _logger;

        public TestCommand(
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<TestCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _runner = runner;
            _logger = logger;
        }

        public override async Task HandleAsync(
            TestCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = _discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .OfType<ITestableProject>()
                .ToArray();

            _logger.Debug($"Test {projects.Length} projects.");
            await _runner.RunAsync(projects, (project, tkn) => project.TestAsync(cfg.Env, cfg.TestFilter, tkn), cfg.Deep, token);
        }
    }

    internal class TestCommandConfiguration
    {
        [Position(1, isRequired: false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired: false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; } = ProjectType.None;

        [Option("tf", isRequired: false)]
        [Help("Tests filter.")]
        public string TestFilter { get; set; } = string.Empty;

        [Option]
        [Help("Environment.")]
        public Env Env { get; set; } = Env.Development;

        [Option("d")]
        [Help("Test dependencies.")]
        public bool Deep { get; set; }
    }
}