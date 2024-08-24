using Annium.Extensions.Arguments;

namespace Xx.Commands.Sync;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "sync";
    public static string Description => "Sync repositories.";

    public Group()
    {
        Add<SyncCommand>();
        Add<SyncListCommand>();
        Add<SyncSetCommand>();
        Add<SyncRemoveCommand>();
        Add<SyncStateCommand>();
    }
}
