using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class RestoreCommand : AsyncCommand<RestoreCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "restore";
        public override string Description { get; } = "Restore tracked registry information.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly IConfigurationManager configurationManager;
        private readonly MainClientFactory mainClientFactory;

        public RestoreCommand(
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
            RestoreCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var user = cfg.User;
            var dir = discoverCfg.Root;

            var configuration = configurationManager.LoadBarebone(dir);
            if (configuration == null)
            {
                Console.WriteLine("Registry can't be restored, because it's not tracked. Track it first");
                return;
            }

            var client = mainClientFactory.Create(configuration.Registry);

            var password = Annium.Extensions.CommandLine.Cli.ReadSecure("Password: ");

            configuration.SetToken(await client.LoginAsync(user, password));
            configuration.SetServers(
                (await client.GetRegistryInfoAsync())
                .ToDictionary(e => ProjectType.Get(e.Key), e => e.Value)
            );

            var projects = discoverTask.Run(discoverCfg).ToArray();

            configurationManager.Save(dir, projects, configuration);

            Console.WriteLine("Registry restored");
        }
    }

    internal class RestoreCommandConfiguration
    {
        [Option(isRequired: true)]
        [Help("User name.")]
        public string User { get; set; }
    }
}