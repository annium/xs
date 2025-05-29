using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Node.Commands.New;

public class Group : Extensions.Arguments.Group, ICommandDescriptor
{
    internal const string TemplatesDir = "Templates";
    internal const string ProjectTemplate = "package_tpl";
    public static string Id => "new";
    public static string Description { get; } = $"Create new {Constants.ProjectType} project.";

    public Group()
    {
        Add<AppReactCommand>();
        Add<LibCommand>();
        Add<LibReactCommand>();
    }
}
