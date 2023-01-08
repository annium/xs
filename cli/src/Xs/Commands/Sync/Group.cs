namespace Xs.Commands.Sync;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id => "sync";
    public override string Description => "Sync repositories.";

    public Group()
    {
        Add<SyncCommand>();
        Add<SyncListCommand>();
        Add<SyncSetCommand>();
        Add<SyncRemoveCommand>();
    }
}