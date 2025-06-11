using Annium.Extensions.Arguments.Commands;

namespace Annium.Xs.Cli.Commands.Sync;

internal class Group : Extensions.Arguments.Commands.Group, ICommandDescriptor
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
