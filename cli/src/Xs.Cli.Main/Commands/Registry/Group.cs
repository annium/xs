namespace Xs.Cli.Main.Commands.Registry
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "registry";

        public override string Description { get; } = "Registry management.";

        public Group()
        {
            Add<User.Group>();
        }
    }
}