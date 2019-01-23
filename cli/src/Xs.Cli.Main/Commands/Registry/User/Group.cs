namespace Xs.Cli.Main.Commands.Registry.User
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "user";

        public override string Description { get; } = "User management.";

        public Group()
        {
            Add<CreateCommand>();
            Add<DeleteCommand>();
            Add<ShowCommand>();
            Add<UpdateCommand>();
        }
    }
}