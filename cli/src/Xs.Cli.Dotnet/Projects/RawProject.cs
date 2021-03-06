using System;
using System.Collections.Generic;
using Xs.Cli.Core.Models;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class RawProject
    {
        public string Name { get; set; } = string.Empty;
        public Core.Models.Version Version { get; set; } = Core.Models.Version.Empty;
        public string Description { get; set; } = string.Empty;
        public TargetFramework TargetFramework { get; set; } = TargetFramework.NetStandard21;
        public OutputType OutputType { get; set; } = OutputType.Library;
        public IEnumerable<Dependency<string>> Projects { get; set; } = Array.Empty<Dependency<string>>();
        public IEnumerable<Dependency<Package>> Packages { get; set; } = Array.Empty<Dependency<Package>>();
        public bool IsPackable { get; set; }
        public bool IsTestProject { get; set; }

        public void Deconstruct(
            out string name,
            out Core.Models.Version version,
            out string description,
            out TargetFramework targetFramework,
            out OutputType outputType,
            out IEnumerable<Dependency<string>> projects,
            out IEnumerable<Dependency<Package>> packages,
            out bool isPackable,
            out bool isTestProject
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
            isTestProject = IsTestProject;
        }
    }
}