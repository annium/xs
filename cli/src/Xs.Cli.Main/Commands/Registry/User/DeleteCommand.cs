using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Registry.Shared.Client;

namespace Xs.Cli.Main.Commands.Registry.User
{
    internal class DeleteCommand : AsyncCommand<DeleteCommandConfiguration>
    {
        public override string Id { get; } = "delete";

        public override string Description { get; } = "delete user from registry";

        private readonly ISharedClientFactory sharedClientFactory;

        public DeleteCommand(
            ISharedClientFactory sharedClientFactory
        )
        {
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            DeleteCommandConfiguration cfg,
            CancellationToken token
        )
        {
            var client = sharedClientFactory.Create(cfg.Registry);
            var name = cfg.User;

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var userToken = await client.LoginUserAsync(name, password);

            await client.DeleteUserAsync(userToken);

            Console.WriteLine($"User {name} deleted");
        }
    }

    internal class DeleteCommandConfiguration
    {
        [Option(isRequired: true)]
        [Help("Registry location")]
        public Uri Registry { get; set; }

        [Option(isRequired: true)]
        [Help("User name")]
        public string User { get; set; }
    }
}