using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.RegistryClient.Main;
using Xs.Tools;

namespace Xs.Commands
{
    internal class SearchCommand : AsyncCommand<SearchCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "search";
        public override string Description { get; } = "Search for packages in tracked registry.";
        private readonly IConfigurationManager _configurationManager;
        private readonly MainClientFactory _mainClientFactory;
        private readonly ILogger<SearchCommand> _logger;

        public SearchCommand(
            IConfigurationManager configurationManager,
            MainClientFactory mainClientFactory,
            ILogger<SearchCommand> logger
        )
        {
            _configurationManager = configurationManager;
            _mainClientFactory = mainClientFactory;
            _logger = logger;
        }

        public override async Task HandleAsync(
            SearchCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var configuration = _configurationManager.Load(discoverCfg.Root);
            if (configuration == null)
            {
                _logger.Warn("Track registry first to search within it.");
                return;
            }

            var client = _mainClientFactory.Create(configuration.Registry);
            var packages = await client.SearchAsync(configuration.Token, cfg.Type.ToString(), cfg.Query);

            foreach (var package in packages)
                Console.WriteLine($"{package.Name}: {package.Version} ({package.Description})");
        }
    }

    internal class SearchCommandConfiguration
    {
        [Position(1)]
        [Help("Project type.")]
        public ProjectType Type { get; set; } = ProjectType.None;

        [Position(2)]
        [Help("Search query.")]
        public string Query { get; set; } = string.Empty;
    }
}