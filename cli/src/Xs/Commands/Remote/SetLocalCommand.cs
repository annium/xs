using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands.Remote
{
    internal class SetLocalCommand : Command<SetLocalCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "set-local";
        public override string Description { get; } = "Set local registry.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly IConfigurationManager configurationManager;

        public SetLocalCommand(
            DiscoverProjectsTask discoverTask,
            IConfigurationManager configurationManager
        )
        {
            this.discoverTask = discoverTask;
            this.configurationManager = configurationManager;
        }

        public override void Handle(
            SetLocalCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = cfg.Registry;
            var dir = discoverCfg.Root;

            var configuration = configurationManager.Load(dir);
            configuration.SetRegistry(location);
            configuration.SetToken(string.Empty);
            configuration.SetServers(ProjectType.List().ToDictionary(type => type, type => location));

            var projects = discoverTask.Run(discoverCfg).ToArray();

            configurationManager.Save(dir, projects, configuration);

            Console.WriteLine("Registry tracking started");
        }
    }

    internal class SetLocalCommandConfiguration
    {
        [Position(1)]
        [Help("Registry location.")]
        public Uri Registry { get; set; } = new Uri("http://localhost");
    }
}