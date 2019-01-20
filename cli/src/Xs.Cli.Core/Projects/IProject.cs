using System.Collections.Generic;
using System.IO;
using Xs.Cli.Core.Models;
using Xs.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface IProject
    {
        ProjectType Type { get; }

        string Name { get; }

        FileInfo File { get; }

        HashSet<IProject> ProjectDependencies { get; }

        HashSet<Dependency> PackageDependencies { get; }

        bool IsRelated(string path);

        void Save();
    }
}