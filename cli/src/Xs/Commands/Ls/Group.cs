namespace Xs.Commands.Ls
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id => "ls";
        public override string Description => "List projects and their dependencies.";

        public Group()
        {
            Add<ListCommand>();
            Add<ListInsCommand>();
            Add<ListOutsCommand>();
        }
    }
}