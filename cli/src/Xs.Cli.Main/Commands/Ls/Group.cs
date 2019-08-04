namespace Xs.Cli.Main.Commands.Ls
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "ls";
        public override string Description { get; } = "List projects and their dependencies.";

        public Group()
        {
            Add<ListCommand>();
            Add<ListInsCommand>();
            Add<ListOutsCommand>();
        }
    }
}