using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class DeleteCommand : Command<CwdCommandConfiguration>
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
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var dir = cwdCfg.Cwd;

            var projects = discoverTask.Run(dir).ToArray();

            configurationManager.Delete(dir, projects);

            Console.WriteLine("Registry tracking stopped.");
        }
    }
}