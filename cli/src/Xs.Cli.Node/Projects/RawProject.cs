using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Node.Projects
{
    internal class RawProject
    {
        public string Name { get; set; }

        public Version Version { get; set; }

        public IEnumerable<string> ProjectDependencies { get; set; }

        public IEnumerable<Dependency> PackageDependencies { get; set; }

        public IReadOnlyDictionary<string, string> Scripts { get; set; }

        public void Deconstruct(
            out string name,
            out Version version,
            out IEnumerable<string> projectDependencies,
            out IEnumerable<Dependency> packageDependencies,
            out IReadOnlyDictionary<string, string> scripts
        )
        {
            name = Name;
            version = Version;
            projectDependencies = ProjectDependencies;
            packageDependencies = PackageDependencies;
            scripts = Scripts;
        }
    }
}