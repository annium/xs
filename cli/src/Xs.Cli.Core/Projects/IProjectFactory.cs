namespace Xs.Cli.Core.Projects
{
    public interface IProjectFactory
    {
        ISpecialProjectFactory ResolveFactory(string directory);

        bool IsProjectFile(string file);
    }
}