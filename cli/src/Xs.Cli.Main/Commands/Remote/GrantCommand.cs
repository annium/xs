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
    internal class GrantCommand : AsyncCommand<GrantCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "grant";
        public override string Description { get; } = "grant permission for package";
        private readonly IConfigurationManager configurationManager;
        private readonly SharedClientFactory sharedClientFactory;

        public GrantCommand(
            IConfigurationManager configurationManager,
            SharedClientFactory sharedClientFactory
        )
        {
            this.configurationManager = configurationManager;
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            GrantCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var registry = configurationManager.Load(cwdCfg.Cwd).Registries
                .FirstOrDefault(e => e.Name.ToLowerInvariant() == cfg.Server.ToLowerInvariant());
            if (registry == null)
                throw new InvalidOperationException($"Registry {cfg.Server} is not tracked. Track it to manipulate permissions.");

            var client = sharedClientFactory.Create(registry.Location);

            await client.Permissions.GrantAsync(cfg.Type, cfg.Name, cfg.Category, cfg.Permission, registry.Token);

            Console.WriteLine($"Granted {cfg.Category} {cfg.Permission.ToString()} permission for {cfg.Type} {cfg.Name} package at {registry.Name} ({registry.Location}).");
        }
    }

    internal class GrantCommandConfiguration
    {
        [Position(1)]
        [Help("Server to grant permissions at.")]
        public string Server { get; set; }

        [Position(2)]
        [Help("Package project type.")]
        public ProjectType Type { get; set; }

        [Position(3)]
        [Help("Package name.")]
        public string Name { get; set; }

        [Position(4)]
        [Help("Permission category, permission will be granted to.")]
        public PermissionCategory Category { get; set; }

        [Position(5)]
        [Help("Permission to be granted.")]
        public Permission Permission { get; set; }
    }
}