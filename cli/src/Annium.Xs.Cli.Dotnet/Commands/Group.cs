using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Dotnet.Commands;

public class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id { get; } = Constants.ProjectType.ToString().ToLowerInvariant();
    public static string Description { get; } = $"{Constants.ProjectType} specific commands.";

    public Group()
    {
        Add<New.Group>();
        Add<Nuget.Group>();
        Add<Sln.Group>();
    }
}
