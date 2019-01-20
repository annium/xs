namespace Xs.Cli.Main.Commands
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "";

        public override string Description { get; } = "xs toolkit";

        public Group()
        {
            // groups
            Add<Ls.Group>();
            Add<Registry.Group>();
            Add<Remote.Group>();

            // commands
            Add<AddCommand>();
            Add<BuildCommand>();
            Add<CleanCommand>();
            Add<DeleteCommand>();
            Add<InstallCommand>();
            Add<SearchCommand>();
            Add<TestCommand>();
            Add<UpdateCommand>();
            Add<UseCommand>();
            Add<WatchCommand>();
        }
    }
}