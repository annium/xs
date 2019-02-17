using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Cli.Main.Commands.Remote
{
    internal class AddCommand : AsyncCommand<AddCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "add";

        public override string Description { get; } = "Start tracking registry.";

        private readonly IConfigurationManager configurationManager;

        private readonly MainClientFactory sharedClientFactory;

        public AddCommand(
            IConfigurationManager configurationManager,
            MainClientFactory sharedClientFactory
        )
        {
            this.configurationManager = configurationManager;
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            AddCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Name;
            var location = cfg.Registry;
            var user = cfg.User;
            var dir = cwdCfg.Cwd;

            var client = sharedClientFactory.Create(location);

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var userToken = await client.LoginAsync(user, password);
            var data = await client.GetRegistryInfoAsync(userToken);

            var registry = new Models.Registry
            {
                Name = name,
                Location = location,
                Token = userToken,
                Servers = data.ToDictionary(e => ProjectType.Get(e.Key), e => e.Value)
            };

            var configuration = configurationManager.Load(dir);
            var index = configuration.Registries.FindIndex(e => e.Name == registry.Name);
            if (index >= 0)
                configuration.Registries[index] = registry;
            else
                configuration.Registries.Add(registry);
            configurationManager.Save(dir, configuration);

            Console.WriteLine($"Registry '{name}' tracking started");
        }
    }

    internal class AddCommandConfiguration
    {
        [Position(1)]
        [Help("Registry name.")]
        public string Name { get; set; }

        [Position(2)]
        [Help("Registry location.")]
        public Uri Registry { get; set; }

        [Option(isRequired: true)]
        [Help("User name.")]
        public string User { get; set; }
    }
}