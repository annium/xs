using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Registry.Shared.Client;

namespace Xs.Cli.Main.Commands.Registry.User
{
    internal class ShowCommand : AsyncCommand<ShowCommandConfiguration>
    {
        public override string Id { get; } = "show";

        public override string Description { get; } = "show user information from registry";

        private readonly SharedClientFactory sharedClientFactory;

        public ShowCommand(
            SharedClientFactory sharedClientFactory
        )
        {
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            ShowCommandConfiguration cfg,
            CancellationToken token
        )
        {
            var client = sharedClientFactory.Create(cfg.Registry);
            var name = cfg.User;

            Console.Write("Password: ");
            var password = Console.ReadLine();

            await client.User.LoginAsync(name, password);
            Console.WriteLine($"User {name} exists");
        }
    }

    internal class ShowCommandConfiguration
    {
        [Option(isRequired: true)]
        [Help("Registry location")]
        public Uri Registry { get; set; }

        [Option(isRequired: true)]
        [Help("User name")]
        public string User { get; set; }
    }
}