using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Commands.Sync;

internal class Group : Extensions.Arguments.Group, ICommandDescriptor
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
