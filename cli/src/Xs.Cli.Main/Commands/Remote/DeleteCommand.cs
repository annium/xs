using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class DeleteCommand : Command<DeleteCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "delete";

        public override string Description { get; } = "Stop tracking registry.";

        private readonly IConfigurationManager configurationManager;

        public DeleteCommand(
            IConfigurationManager configurationManager
        )
        {
            this.configurationManager = configurationManager;
        }

        public override void Handle(
            DeleteCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Name;
            var dir = cwdCfg.Cwd;

            var configuration = configurationManager.Load(dir);
            configuration.Registries.RemoveAll(e => e.Name == name);
            configurationManager.Save(dir, configuration);

            Console.WriteLine($"Registry '{name}' tracking stopped.");
        }
    }

    internal class DeleteCommandConfiguration
    {
        [Position(1)]
        [Help("Registry name.")]
        public string Name { get; set; }
    }
}