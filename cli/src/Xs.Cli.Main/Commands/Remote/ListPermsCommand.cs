using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Tools;
using Xs.Core.Helpers;
using Xs.Core.Models;
using Xs.Registry.Shared.Client;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class ListPermsCommand : AsyncCommand<ListPermsCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "list-perms";
        public override string Description { get; } = "list package permissions";
        private readonly IConfigurationManager configurationManager;
        private readonly SharedClientFactory sharedClientFactory;

        public ListPermsCommand(
            IConfigurationManager configurationManager,
            SharedClientFactory sharedClientFactory
        )
        {
            this.configurationManager = configurationManager;
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            ListPermsCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var registry = configurationManager.Load(cwdCfg.Cwd).Registries
                .FirstOrDefault(e => e.Name.ToLowerInvariant() == cfg.Server.ToLowerInvariant());
            if (registry == null)
                throw new InvalidOperationException($"Registry {cfg.Server} is not tracked. Track it to manipulate permissions");

            var client = sharedClientFactory.Create(registry.Location);

            var permissions = await client.Permissions.GetAsync(cfg.Type, cfg.Name, registry.Token);
            foreach (var(category, perms) in permissions)
                Console.WriteLine($"{category}: {perms.ToString()}");
        }
    }

    internal class ListPermsCommandConfiguration
    {
        [Position(1)]
        [Help("Server to get permissions from.")]
        public string Server { get; set; }

        [Position(2)]
        [Help("Package project type.")]
        public ProjectType Type { get; set; }

        [Position(3)]
        [Help("Package name.")]
        public string Name { get; set; }
    }
}