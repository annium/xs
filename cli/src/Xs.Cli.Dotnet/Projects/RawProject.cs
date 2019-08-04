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
        public IEnumerable<Dependency<string>> Projects { get; set; }
        public IEnumerable<Dependency<Package>> Packages { get; set; }

        public bool IsPackable { get; set; }

        public void Deconstruct(
            out string name,
            out Version version,
            out string description,
            out TargetFramework targetFramework,
            out OutputType outputType,
            out IEnumerable<Dependency<string>> projects,
            out IEnumerable<Dependency<Package>> packages,
            out bool isPackable
        )
        {
            name = Name;
            version = Version;
            description = Description;
            targetFramework = TargetFramework;
            outputType = OutputType;
            projects = Projects;
            packages = Packages;
            isPackable = IsPackable;
        }
    }
}