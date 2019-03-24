using System.Collections.Generic;
using System.IO;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface IProject
    {
        ProjectType Type { get; }

        string Name { get; }

        Version Version { get; }

        string Description { get; }

        FileInfo File { get; }

        HashSet<Dependency<IProject>> Projects { get; }

        HashSet<Dependency<Package>> Packages { get; }

        bool IsRelated(string path);

        void Save();
    }
}