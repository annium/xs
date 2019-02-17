using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Models;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class SetCommand : AsyncCommand<SetCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "set";

        public override string Description { get; } = "Start tracking registry.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly IConfigurationManager configurationManager;

        private readonly MainClientFactory mainClientFactory;

        public SetCommand(
            DiscoverProjectsTask discoverTask,
            IConfigurationManager configurationManager,
            MainClientFactory mainClientFactory
        )
        {
            this.discoverTask = discoverTask;
            this.configurationManager = configurationManager;
            this.mainClientFactory = mainClientFactory;
        }

        public override async Task HandleAsync(
            SetCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var location = cfg.Registry;
            var user = cfg.User;
            var dir = cwdCfg.Cwd;

            var client = mainClientFactory.Create(location);

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var configuration = new Configuration();
            configuration.Location = location;
            configuration.Token = await client.LoginAsync(user, password);
            configuration.Servers = (await client.GetRegistryInfoAsync())
                .ToDictionary(e => ProjectType.Get(e.Key), e => e.Value);

            var projects = (await discoverTask.RunAsync(dir)).ToArray();

            configurationManager.Save(dir, projects, configuration);

            Console.WriteLine("Registry tracking started");
        }
    }

    internal class SetCommandConfiguration
    {
        [Position(1)]
        [Help("Registry location.")]
        public Uri Registry { get; set; }

        [Option(isRequired: true)]
        [Help("User name.")]
        public string User { get; set; }
    }
}