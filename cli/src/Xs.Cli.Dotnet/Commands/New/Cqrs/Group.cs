using Annium.Extensions.Arguments;

namespace Xs.Cli.Dotnet.Commands.New.Cqrs;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    internal const string TemplatesDir = "Templates.CQRS";
    public static string Id => "cqrs";
    public static string Description => "Create CQRS architecture elements.";

    public Group()
    {
        Add<CommandCommand>();
        Add<QueryCommand>();
    }
}
