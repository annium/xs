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
    internal class RevokeCommand : AsyncCommand<RevokeCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "revoke";
        public override string Description { get; } = "Revoke permission for package in registry.";
        private readonly IConfigurationManager configurationManager;
        private readonly SharedClientFactory sharedClientFactory;

        public RevokeCommand(
            IConfigurationManager configurationManager,
            SharedClientFactory sharedClientFactory
        )
        {
            this.configurationManager = configurationManager;
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            RevokeCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var registry = configurationManager.Load(cwdCfg.Cwd).Registries
                .FirstOrDefault(e => e.Name.ToLowerInvariant() == cfg.Registry.ToLowerInvariant());
            if (registry == null)
                throw new InvalidOperationException($"Registry {cfg.Registry} is not tracked. Track it to manipulate permissions.");

            var client = sharedClientFactory.Create(registry.Location);

            await client.Permissions.RevokeAsync(cfg.Type, cfg.Name, cfg.Category, cfg.Permission, registry.Token);

            Console.WriteLine($"Revoked {cfg.Category} {cfg.Permission.ToString()} permission for {cfg.Type} {cfg.Name} package at {registry.Name} ({registry.Location}).");
        }
    }

    internal class RevokeCommandConfiguration
    {
        [Position(1)]
        [Help("Tracked registry name to revoke permissions at.")]
        public string Registry { get; set; }

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