using Annium.Extensions.Arguments.Commands;

namespace Annium.Xs.Cli.Node.Commands;

public class Group : Extensions.Arguments.Commands.Group, ICommandDescriptor
{
    public static string Id { get; } = Constants.ProjectType.ToString().ToLowerInvariant();
    public static string Description { get; } = $"{Constants.ProjectType} specific commands.";

    public Group()
    {
        Add<New.Group>();
    }
}
