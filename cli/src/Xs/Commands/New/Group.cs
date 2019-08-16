namespace Xs.Commands.New
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "new";
        public override string Description { get; } = "Create new project.";

        public Group()
        {
            Add<Cli.Dotnet.Commands.New.Group>();
            Add<Cli.Node.Commands.New.Group>();
        }
    }
}