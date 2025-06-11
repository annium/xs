using Annium.Extensions.Arguments.Commands;

namespace Annium.Xs.Cli.Commands.Ls;

internal class Group : Extensions.Arguments.Commands.Group, ICommandDescriptor
{
    public static string Id => "ls";
    public static string Description => "List projects and their dependencies.";

    public Group()
    {
        Add<ListCommand>();
        Add<ListInsCommand>();
        Add<ListOutsCommand>();
    }
}
