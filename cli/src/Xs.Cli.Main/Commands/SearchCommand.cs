using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Cli.Main.Commands
{
    internal class SearchCommand : AsyncCommand<SearchCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "search";

        public override string Description { get; } = "Search for packages in tracked registry.";

        private readonly IConfigurationManager configurationManager;

        private readonly MainClientFactory mainClientFactory;

        private readonly ILogger<SearchCommand> logger;

        public SearchCommand(
            IConfigurationManager configurationManager,
            MainClientFactory mainClientFactory,
            ILogger<SearchCommand> logger
        )
        {
            this.configurationManager = configurationManager;
            this.mainClientFactory = mainClientFactory;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            SearchCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var type = cfg.Type;

            var configuration = await configurationManager.LoadAsync(discoverCfg.Root);
            if (configuration == null)
            {
                logger.Warn("Track registry first to search within it.");
                return;
            }

            var client = mainClientFactory.Create(configuration.Registry);
            var packages = await client.SearchAsync(configuration.Token, cfg.Type.ToString(), cfg.Query);

            foreach (var package in packages)
                Console.WriteLine($"{package.Name}: {package.Version} ({package.Description})");
        }
    }

    internal class SearchCommandConfiguration
    {
        [Position(1)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Position(2)]
        [Help("Search query.")]
        public string Query { get; set; }
    }
}