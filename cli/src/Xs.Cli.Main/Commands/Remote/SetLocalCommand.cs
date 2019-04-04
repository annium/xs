using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Models;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class SetLocalCommand : Command<SetLocalCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "set-local";

        public override string Description { get; } = "Set local registry.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly IConfigurationManager configurationManager;

        private readonly MainClientFactory mainClientFactory;

        public SetLocalCommand(
            DiscoverProjectsTask discoverTask,
            IConfigurationManager configurationManager,
            MainClientFactory mainClientFactory
        )
        {
            this.discoverTask = discoverTask;
            this.configurationManager = configurationManager;
            this.mainClientFactory = mainClientFactory;
        }

        public override void Handle(
            SetLocalCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = cfg.Registry;
            var dir = discoverCfg.Root;

            var configuration = new Configuration();
            configuration.Location = location;
            configuration.Token = string.Empty;
            configuration.Servers = ProjectType.List().ToDictionary(type => type, type => location);

            var projects = discoverTask.Run(discoverCfg).ToArray();

            configurationManager.Save(dir, projects, configuration);

            Console.WriteLine("Registry tracking started");
        }
    }

    internal class SetLocalCommandConfiguration
    {
        [Position(1)]
        [Help("Registry location.")]
        public Uri Registry { get; set; }
    }
}