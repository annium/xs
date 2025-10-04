using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Commands.Ls;

internal class Group : Extensions.Arguments.Group, ICommandDescriptor
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
