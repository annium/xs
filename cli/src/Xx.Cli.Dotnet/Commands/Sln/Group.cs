using Annium.Extensions.Arguments;

namespace Xx.Cli.Dotnet.Commands.Sln;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id { get; } = "sln";
    public static string Description { get; } = $"Work with {Constants.ProjectType} solution.";

    public Group()
    {
        Add<RemoveCommand>();
        Add<SetCommand>();
        Add<SyncCommand>();
    }
}
