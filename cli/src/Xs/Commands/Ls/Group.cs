using Annium.Extensions.Arguments;

namespace Xs.Commands.Ls;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
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
