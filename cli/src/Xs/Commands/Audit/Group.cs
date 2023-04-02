using Annium.Extensions.Arguments;

namespace Xs.Commands.Audit;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "audit";

    public static string Description => "Audit projects.";

    public Group()
    {
        Add<AuditCommand>();
        Add<AuditRulesCommand>();
    }
}