using System;
using System.Collections.Generic;
using Xx.Cli.Core.Models;
using Xx.Cli.Dotnet.Models;
using Version = Xx.Cli.Core.Models.Version;

namespace Xx.Cli.Dotnet.Projects;

internal class RawProject
{
    public string Name { get; set; } = string.Empty;
    public Version Version { get; set; } = Version.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Solutions { get; set; } = Array.Empty<string>();
    public TargetFramework TargetFramework { get; set; } = TargetFramework.NetStandard21;
    public OutputType OutputType { get; set; } = OutputType.Library;
    public IEnumerable<Dependency<string>> Projects { get; set; } = Array.Empty<Dependency<string>>();
    public IEnumerable<Dependency<Package>> Packages { get; set; } = Array.Empty<Dependency<Package>>();
    public bool IsPackable { get; set; }
    public bool IsTestProject { get; set; }

    public void Deconstruct(
        out string name,
        out Version version,
        out string description,
        out IReadOnlyCollection<string> solutions,
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
        solutions = Solutions;
        targetFramework = TargetFramework;
        outputType = OutputType;
        projects = Projects;
        packages = Packages;
        isPackable = IsPackable;
        isTestProject = IsTestProject;
    }
}
