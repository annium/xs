using System.Collections.Generic;
using Xs.Cli.Core.Models;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class RawProject
    {
        public string Name { get; set; }

        public Version Version { get; set; }
        
        public string Description { get; set; }

        public TargetFramework TargetFramework { get; set; }

        public OutputType OutputType { get; set; }

        public IEnumerable<string> ProjectDependencies { get; set; }

        public IEnumerable<Dependency> PackageDependencies { get; set; }

        public void Deconstruct(
            out string name,
            out Version version,
            out string description,
            out TargetFramework targetFramework,
            out OutputType outputType,
            out IEnumerable<string> projectDependencies,
            out IEnumerable<Dependency> packageDependencies
        )
        {
            name = Name;
            version = Version;
            description = Description;
            targetFramework = TargetFramework;
            outputType = OutputType;
            projectDependencies = ProjectDependencies;
            packageDependencies = PackageDependencies;
        }
    }
}