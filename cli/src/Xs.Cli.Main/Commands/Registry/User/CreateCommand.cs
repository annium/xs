using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Registry.Shared.Client;

namespace Xs.Cli.Main.Commands.Registry.User
{
    internal class CreateCommand : AsyncCommand<CreateCommandConfiguration>
    {
        public override string Id { get; } = "create";

        public override string Description { get; } = "create user in registry";

        private readonly ISharedClientFactory sharedClientFactory;

        public CreateCommand(
            ISharedClientFactory sharedClientFactory
        )
        {
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            CreateCommandConfiguration cfg,
            CancellationToken token
        )
        {
            var client = sharedClientFactory.Create(cfg.Registry);
            var name = cfg.User;

            Console.Write("Password: ");
            var password = Console.ReadLine();

            await client.CreateUserAsync(name, password);
            Console.WriteLine($"User {name} created");
        }
    }

    internal class CreateCommandConfiguration
    {
        [Option(isRequired: true)]
        [Help("Registry location")]
        public Uri Registry { get; set; }

        [Option(isRequired: true)]
        [Help("User name")]
        public string User { get; set; }
    }
}