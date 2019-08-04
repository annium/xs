using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class ShowCommand : AsyncCommand<DiscoverConfiguration>
    {
        public override string Id { get; } = "show";
        public override string Description { get; } = "Show information about tracked registry.";
        private readonly IConfigurationManager configurationManager;

        public ShowCommand(
            IConfigurationManager configurationManager
        )
        {
            this.configurationManager = configurationManager;
        }

        public override async Task HandleAsync(
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var dir = discoverCfg.Root;

            var configuration = await configurationManager.LoadAsync(dir);

            if (configuration == null)
                Console.WriteLine("Registry is not tracked.");
            else
                Console.Write(Yaml.Serializer.Serialize(configuration));
        }
    }
}