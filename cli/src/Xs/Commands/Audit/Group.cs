namespace Xs.Commands.Audit;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id => "audit";

    public override string Description => "Audit projects.";

    public Group()
    {
        Add<AuditCommand>();
        Add<AuditRulesCommand>();
    }
}