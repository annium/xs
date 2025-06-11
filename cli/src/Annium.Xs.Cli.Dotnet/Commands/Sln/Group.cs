using Annium.Extensions.Arguments.Commands;

namespace Annium.Xs.Cli.Dotnet.Commands.Sln;

public class Group : Annium.Extensions.Arguments.Commands.Group, ICommandDescriptor
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
