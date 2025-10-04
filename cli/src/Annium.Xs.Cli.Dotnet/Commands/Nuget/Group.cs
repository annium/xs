using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Dotnet.Commands.Nuget;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id { get; } = "nuget";
    public static string Description { get; } = "NuGet extended operations";

    public Group()
    {
        Add<ListContentsCommand>();
    }
}
