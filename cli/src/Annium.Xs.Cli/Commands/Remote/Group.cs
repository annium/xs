using Annium.Extensions.Arguments.Commands;

namespace Annium.Xs.Cli.Commands.Remote;

internal class Group : Extensions.Arguments.Commands.Group, ICommandDescriptor
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
