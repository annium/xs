namespace Xs.Cli.Main.Commands.Remote
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "remote";

        public override string Description { get; } = "manage tracked registries";

        public Group()
        {
            Add<AddCommand>();
            Add<DeleteCommand>();
            Add<ListCommand>();
            Add<ShowCommand>();
        }
    }
}