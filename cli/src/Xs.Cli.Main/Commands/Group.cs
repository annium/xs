namespace Xs.Cli.Main.Commands
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "xs";

        public override string Description { get; } = "xs toolkit";

        public Group()
        {
            // groups
            Add<Audit.Group>();
            Add<Ls.Group>();
            Add<Remote.Group>();
            Add<New.Group>();

            // commands
            Add<AddCommand>();
            Add<BuildCommand>();
            Add<CleanCommand>();
            Add<DeleteCommand>();
            Add<InstallCommand>();
            Add<PublishCommand>();
            Add<SearchCommand>();
            Add<TestCommand>();
            Add<UnpublishCommand>();
            Add<UpdateCommand>();
            Add<UseCommand>();
            Add<WatchCommand>();
        }
    }
}