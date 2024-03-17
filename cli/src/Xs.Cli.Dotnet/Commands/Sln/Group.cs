using Annium.Extensions.Arguments;

namespace Xs.Cli.Dotnet.Commands.Sln;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id { get; } = Constants.ProjectType.ToString().ToLowerInvariant();
    public static string Description { get; } = $"Work with {Constants.ProjectType} solution.";

    public Group()
    {
        Add<SyncCommand>();
    }
}
