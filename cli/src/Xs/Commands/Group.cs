using Annium.Extensions.Arguments;

namespace Xs.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
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
        Add<Cli.Dotnet.Commands.Group>();
        Add<Cli.Node.Commands.Group>();

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