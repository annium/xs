namespace Xs.Cli.Main.Commands.Remote
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "remote";

        public override string Description { get; } = "Manage tracked registries.";

        public Group()
        {
            Add<AddCommand>();
            Add<DeleteCommand>();
            Add<GrantCommand>();
            Add<ListCommand>();
            Add<ShowPermsCommand>();
            Add<RevokeCommand>();
            Add<ShowCommand>();
        }
    }
}