using System;
using System.Linq;
using System.Threading;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands.Remote
{
    internal class DeleteCommand : Command<DiscoverConfiguration>
    {
        public override string Id => "delete";
        public override string Description => "Stop tracking registry.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly IConfigurationManager _configurationManager;

        public DeleteCommand(
            DiscoverProjectsTask discoverTask,
            IConfigurationManager configurationManager
        )
        {
            _discoverTask = discoverTask;
            _configurationManager = configurationManager;
        }

        public override void Handle(
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            var dir = discoverCfg.Root;

            var projects = _discoverTask.RunAsync(discoverCfg).Await().ToArray();

            _configurationManager.Delete(dir, projects);

            Console.WriteLine("Registry tracking stopped.");
        }
    }
}