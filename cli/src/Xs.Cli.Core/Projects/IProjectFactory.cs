using System.Collections.Generic;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface IProjectFactory
    {
        ISpecialProjectFactory FindFactory(string directory);

        bool IsProjectFile(string file);

        IProject CreateProject(
            string directory,
            ISpecialProjectFactory factory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies,
            DiscoverConfiguration configuration
        );
    }
}