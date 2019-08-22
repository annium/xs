using Xs.Cli.Core.Commands;

namespace Xs.Cli.Core.Projects
{
    public interface IProjectFactory
    {
        ISpecialProjectFactory FindFactory(string directory);

        bool IsProjectFile(string file);

        IProject CreateProject(
            string directory,
            ISpecialProjectFactory factory,
            DiscoverConfiguration configuration
        );
    }
}