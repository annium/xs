using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Cli.Main.Commands
{
    internal class SearchCommand : AsyncCommand<SearchCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "search";

        public override string Description { get; } = "Search for packages in tracked registry.";

        private readonly IConfigurationManager configurationManager;

        private readonly MainClientFactory mainClientFactory;

        private readonly ILogger logger;

        public SearchCommand(
            IConfigurationManager configurationManager,
            MainClientFactory mainClientFactory,
            ILogger logger
        )
        {
            this.configurationManager = configurationManager;
            this.mainClientFactory = mainClientFactory;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            SearchCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var type = cfg.Type;

            var configuration = await configurationManager.Load(cwdCfg.Cwd);
            if (configuration == null)
            {
                logger.LogWarn("Track registry first to search within it.");
                return;
            }

            var client = mainClientFactory.Create(configuration.Location);
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