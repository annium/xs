using Annium.Extensions.Arguments.Commands;

namespace Annium.Xs.Cli.Commands.Audit;

internal class Group : Extensions.Arguments.Commands.Group, ICommandDescriptor
{
    public static string Id => "audit";

    public static string Description => "Audit projects.";

    public Group()
    {
        Add<AuditCommand>();
        Add<AuditRulesCommand>();
    }
}
