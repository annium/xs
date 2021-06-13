namespace Xs.Cli.Dotnet.Commands.New.Cqrs
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        internal const string TemplatesDir = "Templates.CQRS";
        public override string Id => "cqrs";
        public override string Description => "Create CQRS architecture elements.";

        public Group()
        {
            Add<CommandCommand>();
            Add<QueryCommand>();
        }
    }
}