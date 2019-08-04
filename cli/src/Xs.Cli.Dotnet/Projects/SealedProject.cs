namespace Xs.Cli.Dotnet.Projects
{
    internal class SealedProject : SpecialProject<SealedProject>
    {
        public SealedProject(SpecialProjectContext<SealedProject> context) : base(context) { }
    }
}