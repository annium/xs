using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Commands.Remote
{
    internal class SetCommand : AsyncCommand<SetCommandConfiguration, DiscoverConfiguration>
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
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = cfg.Registry;
            var user = cfg.User;
            var dir = discoverCfg.Root;

            var client = mainClientFactory.Create(location);

            var password = Annium.Extensions.CommandLine.Cli.ReadSecure("Password: ");

            var configuration = configurationManager.LoadBarebone(dir) ?? new Configuration();
            configuration.SetRegistry(location);
            configuration.SetToken(await client.LoginAsync(user, password));
            configuration.SetServers(
                (await client.GetRegistryInfoAsync())
                .Servers
                .ToDictionary(e => ProjectType.Get(e.Key), e => e.Value)
            );

            var projects = discoverTask.Run(discoverCfg).ToArray();

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