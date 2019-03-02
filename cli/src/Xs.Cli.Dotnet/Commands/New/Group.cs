namespace Xs.Cli.Dotnet.Commands.New
{
    public class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = Constants.ProjectType.ToString();

        public override string Description { get; } = $"Create new {Constants.ProjectType.ToString()} project.";

        public Group()
        {
            Add<LibCommand>();
        }
    }
}