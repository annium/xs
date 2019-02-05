using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class ShowCommand : Command<ShowCommandConfiguration, CwdCommandConfiguration>
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
            ShowCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Name;
            var dir = cwdCfg.Cwd;

            var configuration = configurationManager.Load(dir);
            var registry = configuration.Registries.FirstOrDefault(e => e.Name == name);

            if (registry == null)
                Console.WriteLine($"Registry '{name}' is not tracked.");
            else
                Console.WriteLine(Json.Write(registry));
        }
    }

    internal class ShowCommandConfiguration
    {
        [Position(1)]
        [Help("Registry name.")]
        public string Name { get; set; }
    }
}