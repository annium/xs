namespace Xs.Cli.Dotnet.Commands.New
{
    public class Group : Annium.Extensions.Arguments.Group
    {
        internal const string TemplatesDir = "Templates";
        internal const string ProjectTemplate = "project_tpl";
        public override string Id { get; } = "new";
        public override string Description { get; } = $"Create new {Constants.ProjectType.ToString()} project.";

        public Group()
        {
            Add<Cqrs.Group>();
            Add<ClassCommand>();
            Add<InterfaceCommand>();
            Add<ExeCommand>();
            Add<LibCommand>();
            Add<LibTestsCommand>();
            Add<WebCommand>();
            Add<WebTestsCommand>();
        }
    }
}