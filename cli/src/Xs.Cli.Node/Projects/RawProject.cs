using System;
using System.Collections.Generic;
using Xs.Cli.Core.Models;
using Version = Xs.Cli.Core.Models.Version;

namespace Xs.Cli.Node.Projects;

internal class RawProject
{
    public string Name { get; set; } = string.Empty;
    public Version Version { get; set; } = Version.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<Dependency<string>> Projects { get; set; } = Array.Empty<Dependency<string>>();
    public IEnumerable<Dependency<Package>> Packages { get; set; } = Array.Empty<Dependency<Package>>();
    public IReadOnlyDictionary<string, string> Scripts { get; set; } = new Dictionary<string, string>();
    public bool IsPackable { get; set; }

    public void Deconstruct(
        out string name,
        out Version version,
        out string description,
        out IEnumerable<Dependency<string>> projects,
        out IEnumerable<Dependency<Package>> packages,
        out IReadOnlyDictionary<string, string> scripts,
        out bool isPackable
    )
    {
        name = Name;
        version = Version;
        description = Description;
        projects = Projects;
        packages = Packages;
        scripts = Scripts;
        isPackable = IsPackable;
    }
}
