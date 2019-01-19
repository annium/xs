using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface IProjectFactory
    {
        bool IsProjectDirectory(string directory);

        bool IsProjectFile(string file);

        bool IsTrackablePath(string path);

        IProject CreateProject(
            string directory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies
        );
    }
}