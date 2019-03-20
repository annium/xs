using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class DeleteCommand : Command<DiscoverConfiguration>
    {
        public override string Id { get; } = "delete";

        public override string Description { get; } = "Stop tracking registry.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly IConfigurationManager configurationManager;

        public DeleteCommand(
            DiscoverProjectsTask discoverTask,
            IConfigurationManager configurationManager
        )
        {
            this.discoverTask = discoverTask;
            this.configurationManager = configurationManager;
        }

        public override void Handle(
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var dir = discoverCfg.Root;

            var projects = discoverTask.Run(discoverCfg).ToArray();

            configurationManager.Delete(dir, projects);

            Console.WriteLine("Registry tracking stopped.");
        }
    }
}