using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface ISpecialProjectFactory
    {
        ProjectType Type { get; }

        bool IsProjectDirectory(string directory);

        bool IsProjectFile(string file);

        IProject CreateProject(
            string directory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies
        );
    }
}