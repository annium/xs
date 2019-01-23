using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Tools;
using Xs.Core.Models;
using Xs.Registry.Core.Client;

namespace Xs.Cli.Main.Commands
{
    internal class SearchCommand : AsyncCommand<SearchCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "search";
        public override string Description { get; } = "Search for packages in tracked registries.";
        private readonly IEnumerable<IProjectClientFactory> projectClientFactories;
        private readonly IConfigurationManager configurationManager;
        private readonly ILogger logger;

        public SearchCommand(
            IEnumerable<IProjectClientFactory> projectClientFactories,
            IConfigurationManager configurationManager,
            ILogger logger
        )
        {
            this.projectClientFactories = projectClientFactories;
            this.configurationManager = configurationManager;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            SearchCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var server = cfg.Server;
            var type = cfg.Type;
            var query = cfg.Query;

            var factory = projectClientFactories.FirstOrDefault(e => e.ProjectType == type);
            if (factory == null)
            {
                logger.LogWarn($"No client registered for project type '{type}'.");
                return;
            }

            var registry = configurationManager.Load(cwdCfg.Cwd).Registries
                .FirstOrDefault(e => e.Name.ToLowerInvariant() == server.ToLowerInvariant());
            if (registry == null)
                throw new InvalidOperationException($"Registry {server} is not tracked. Track it to manipulate permissions.");
            if (!registry.Servers.ContainsKey(type))
                throw new InvalidOperationException($"Registry {server} doesn't support project type '{type}'.");

            var client = factory.Create(registry.Servers[type]);

            var results = await client.Info.SearchAsync(query, registry.Token);
            foreach (var(name, version) in results)
                Console.WriteLine($"{name}: {version}");
        }
    }

    internal class SearchCommandConfiguration
    {
        [Position(1)]
        [Help("Tracked registry name to search packages at.")]
        public string Server { get; set; }

        [Position(2)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Position(3)]
        [Help("Search query.")]
        public string Query { get; set; }
    }
}