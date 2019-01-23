using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class ListCommand : Command<CwdCommandConfiguration>
    {
        public override string Id { get; } = "list";

        public override string Description { get; } = "List tracked registries.";

        private readonly IConfigurationManager configurationManager;

        public ListCommand(
            IConfigurationManager configurationManager
        )
        {
            this.configurationManager = configurationManager;
        }

        public override void Handle(
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var dir = cwdCfg.Cwd;

            var configuration = configurationManager.Load(dir);
            foreach (var registry in configuration.Registries)
                Console.WriteLine(registry.Name);
        }
    }
}