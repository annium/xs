using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands
{
    internal class PublishCommand : AsyncCommand<PublishCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "publish";
        public override string Description { get; } = "Publish packages to registry.";
        private readonly IConfigurationManager _configurationManager;
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ProjectsRunner _runner;
        private readonly ILogger<PublishCommand> _logger;

        public PublishCommand(
            IConfigurationManager configurationManager,
            DiscoverProjectsTask discoverTask,
            ProjectsRunner runner,
            ILogger<PublishCommand> logger
        )
        {
            _configurationManager = configurationManager;
            _discoverTask = discoverTask;
            _runner = runner;
            _logger = logger;
        }

        public override async Task HandleAsync(
            PublishCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            var configuration = _configurationManager.Load(discoverCfg.Root);
            if (configuration == null)
                throw new InvalidOperationException("Registry is not tracked. Track it to publish.");

            var projects = _discoverTask.RunAsync(discoverCfg).Await()
                .FilterMask(cfg.Mask)
                .OfType<IPublishableProject>()
                .ToArray();

            if (projects.Length == 0)
            {
                _logger.Info($"No projects found publish.");
                return;
            }

            foreach (var project in projects)
                if (!configuration.Servers.ContainsKey(project.Type))
                    throw new InvalidOperationException($"Registry doesn't support project type '{project.Type}'.");

            _logger.Debug($"Publish {projects.Length} projects.");
            await _runner.RunAsync(
                projects,
                (project, tkn) => project.PublishAsync(configuration.Servers[project.Type], configuration.Token, cfg.Version, tkn),
                cfg.Deep,
                ct
            );
        }
    }

    internal class PublishCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = string.Empty;

        [Position(2)]
        [Help("Version to publish.")]
        public Cli.Core.Models.Version Version { get; set; } = Cli.Core.Models.Version.Empty;

        [Option("d")]
        [Help("Publish dependencies.")]
        public bool Deep { get; set; }
    }
}