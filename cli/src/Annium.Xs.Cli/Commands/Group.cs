using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Commands;

internal class Group : Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xs";
    public static string Description => "xs toolkit";

    public Group()
    {
        // groups
        Add<Audit.Group>();
        Add<Ls.Group>();
        Add<Remote.Group>();
        Add<Sync.Group>();
        Add<Dotnet.Commands.Group>();
        Add<Node.Commands.Group>();

        // commands
        Add<AddCommand>();
        Add<BuildCommand>();
        Add<CleanCommand>();
        Add<DeleteCommand>();
        Add<DepsGraphCommand>();
        Add<FormatCommand>();
        Add<InstallCommand>();
        Add<LinkCommand>();
        Add<MoveCommand>();
        Add<PublishCommand>();
        Add<SearchCommand>();
        Add<TestCommand>();
        Add<UnlinkCommand>();
        Add<UnpublishCommand>();
        Add<UpdateCommand>();
        Add<UseCommand>();
        Add<WatchCommand>();
    }
}
