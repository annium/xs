namespace Xs.Cli.Node.Commands
{
    public class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = Constants.ProjectType.ToString();
        public override string Description { get; } = $"{Constants.ProjectType} specific commands.";

        public Group()
        {
            Add<New.Group>();
        }
    }
}