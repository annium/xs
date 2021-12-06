namespace Xs.Commands.Remote;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id => "remote";
    public override string Description => "Manage tracked registries.";

    public Group()
    {
        Add<DeleteCommand>();
        Add<RestoreCommand>();
        Add<SetCommand>();
        Add<SetLocalCommand>();
        Add<ShowCommand>();
    }
}