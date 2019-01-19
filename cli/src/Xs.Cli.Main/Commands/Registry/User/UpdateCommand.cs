using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Registry.Shared.Client;

namespace Xs.Cli.Main.Commands.Registry.User
{
    internal class UpdateCommand : AsyncCommand<UpdateCommandConfiguration>
    {
        public override string Id { get; } = "update";

        public override string Description { get; } = "update user in registry";

        private readonly SharedClientFactory sharedClientFactory;

        public UpdateCommand(
            SharedClientFactory sharedClientFactory
        )
        {
            this.sharedClientFactory = sharedClientFactory;
        }

        public override async Task HandleAsync(
            UpdateCommandConfiguration cfg,
            CancellationToken token
        )
        {
            var client = sharedClientFactory.Create(cfg.Registry);
            var name = cfg.User;

            Console.Write("Password: ");
            var password = Console.ReadLine();

            var userToken = await client.User.LoginAsync(name, password);

            Console.Write("New password: ");
            var newPassword = Console.ReadLine();

            await client.User.UpdateAsync(userToken, newPassword);

            Console.WriteLine($"User {name} password changed");
        }
    }

    internal class UpdateCommandConfiguration
    {
        [Option(isRequired: true)]
        [Help("Registry location")]
        public Uri Registry { get; set; }

        [Option(isRequired: true)]
        [Help("User name")]
        public string User { get; set; }
    }
}