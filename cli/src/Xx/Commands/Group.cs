using Annium.Extensions.Arguments;

namespace Xx.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xx";
    public static string Description => "xx toolkit";

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
