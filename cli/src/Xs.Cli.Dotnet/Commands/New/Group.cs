using Annium.Extensions.Arguments;

namespace Xs.Cli.Dotnet.Commands.New;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    internal const string TemplatesDir = "Templates";
    internal const string ProjectTemplate = "project_tpl";
    public static string Id => "new";
    public static string Description { get; } = $"Create new {Constants.ProjectType} project.";

    public Group()
    {
        Add<Cqrs.Group>();
        Add<ClassCommand>();
        Add<InterfaceCommand>();
        Add<ExeCommand>();
        Add<LibCommand>();
        Add<LibTestsCommand>();
        Add<WebAssemblyAppCommand>();
        Add<WebAssemblyLibCommand>();
        Add<WebCommand>();
        Add<WebTestsCommand>();
    }
}