namespace Xs.Cli.Node.Commands.New
{
    public class Group : Annium.Extensions.Arguments.Group
    {
        internal const string TemplatesDir = "Templates";

        internal const string ProjectTemplate = "package_tpl";

        public override string Id { get; } = Constants.ProjectType.ToString();

        public override string Description { get; } = $"Create new {Constants.ProjectType.ToString()} project.";

        public Group()
        {
            Add<AppReactCommand>();
            Add<LibCommand>();
            Add<LibReactCommand>();
        }
    }
}