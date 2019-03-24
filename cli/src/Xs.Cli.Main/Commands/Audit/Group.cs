namespace Xs.Cli.Main.Commands.Audit
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "audit";

        public override string Description { get; } = "Audit projects.";

        public Group()
        {
            Add<AuditCommand>();
            Add<AuditRulesCommand>();
        }
    }
}