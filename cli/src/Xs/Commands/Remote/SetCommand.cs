using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.RegistryClient.Main;
using Xs.Tools;

namespace Xs.Commands.Remote
{
    internal class SetCommand : AsyncCommand<SetCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "set";
        public override string Description { get; } = "Start tracking registry.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly IConfigurationManager _configurationManager;
        private readonly MainClientFactory _mainClientFactory;

        public SetCommand(
            DiscoverProjectsTask discoverTask,
            IConfigurationManager configurationManager,
            MainClientFactory mainClientFactory
        )
        {
            _discoverTask = discoverTask;
            _configurationManager = configurationManager;
            _mainClientFactory = mainClientFactory;
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

            var client = _mainClientFactory.Create(location);

            var password = Annium.Extensions.CommandLine.Cli.ReadSecure("Password: ");

            var configuration = _configurationManager.Load(dir);
            configuration.SetRegistry(location);
            configuration.SetToken(await client.LoginAsync(user, password));
            configuration.SetServers(
                (await client.GetRegistryInfoAsync())
                .Servers
                .ToDictionary(e => ProjectType.Get(e.Key), e => e.Value)
            );

            var projects = _discoverTask.Run(discoverCfg).ToArray();

            _configurationManager.Save(dir, projects, configuration);

            Console.WriteLine("Registry tracking started");
        }
    }

    internal class SetCommandConfiguration
    {
        [Position(1)]
        [Help("Registry location.")]
        public Uri Registry { get; set; } = new Uri("http://localhost");

        [Option(isRequired: true)]
        [Help("User name.")]
        public string User { get; set; } = string.Empty;
    }
}