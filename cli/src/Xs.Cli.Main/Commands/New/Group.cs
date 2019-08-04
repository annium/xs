namespace Xs.Cli.Main.Commands.New
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "new";
        public override string Description { get; } = "Create new project.";

        public Group()
        {
            Add<Dotnet.Commands.New.Group>();
            Add<Node.Commands.New.Group>();
        }
    }
}