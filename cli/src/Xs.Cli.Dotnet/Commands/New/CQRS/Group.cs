namespace Xs.Cli.Dotnet.Commands.New.CQRS
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        internal const string TemplatesDir = "Templates.CQRS";
        public override string Id { get; } = "cqrs";
        public override string Description { get; } = "Create CQRS architecture elements.";

        public Group()
        {
            Add<CommandCommand>();
            Add<QueryCommand>();
        }
    }
}