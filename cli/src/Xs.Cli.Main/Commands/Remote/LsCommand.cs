using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class LsCommand : AsyncCommand<CwdCommandConfiguration>
    {
        public override string Id { get; } = "ls";

        public override string Description { get; } = "List tracked registries.";

        private readonly IConfigurationManager configurationManager;

        public LsCommand(
            IConfigurationManager configurationManager
        )
        {
            this.configurationManager = configurationManager;
        }

        public override async Task HandleAsync(
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var dir = cwdCfg.Cwd;

            var configuration = await configurationManager.Load(dir);
            foreach (var registry in configuration.Registries)
                Console.WriteLine(registry.Name);
        }
    }
}