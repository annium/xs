using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Helpers;
using Xs.Tools;

namespace Xs.Commands.Remote
{
    internal class ShowCommand : Command<DiscoverConfiguration>
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

        public override void Handle(
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var dir = discoverCfg.Root;

            var configuration = configurationManager.Load(dir);

            if (configuration == null)
                Console.WriteLine("Registry is not tracked.");
            else
                Console.Write(Yaml.Serializer.Serialize(configuration));
        }
    }
}