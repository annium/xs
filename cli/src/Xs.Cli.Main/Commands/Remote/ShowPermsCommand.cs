using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Tools;
using Xs.Core.Models;
using Xs.Registry.Shared.Client;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class ShowPermsCommand : AsyncCommand<ShowPermsCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "show-perms";
        public override string Description { get; } = "Show permissions for package in registry.";
        private readonly IConfigurationManager configurationManager;
        private readonly SharedClientFactory sharedClientFactory;

        public ShowPermsCommand(
            IConfigurationManager configurationManager,
            SharedClientFactory sharedClientFactory
        )
        {
            this.configurationManager = configurationManager;
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            ShowPermsCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var registry = configurationManager.Load(cwdCfg.Cwd).Registries
                .FirstOrDefault(e => e.Name.ToLowerInvariant() == cfg.Registry.ToLowerInvariant());
            if (registry == null)
                throw new InvalidOperationException($"Registry {cfg.Registry} is not tracked. Track it to manipulate permissions.");

            var client = sharedClientFactory.Create(registry.Location);

            var permissions = await client.Permissions.GetAsync(cfg.Type, cfg.Name, registry.Token);
            foreach (var(category, perms) in permissions)
                Console.WriteLine($"{category}: {perms.ToString()}");
        }
    }

    internal class ShowPermsCommandConfiguration
    {
        [Position(1)]
        [Help("Tracked registry name to show permissions from.")]
        public string Registry { get; set; }

        [Position(2)]
        [Help("Package project type.")]
        public ProjectType Type { get; set; }

        [Position(3)]
        [Help("Package name.")]
        public string Name { get; set; }
    }
}