using Annium.Extensions.Arguments;

namespace Xx.Commands.Remote;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "remote";
    public static string Description => "Manage tracked registries.";

    public Group()
    {
        Add<DeleteCommand>();
        Add<RestoreCommand>();
        Add<SetCommand>();
        Add<SetLocalCommand>();
        Add<ShowCommand>();
    }
}
