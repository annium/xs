using System;
using System.Collections.Generic;
using Xs.Cli.Core.Models;
using Version = Xs.Cli.Core.Models.Version;

namespace Xs.Cli.Core.Projects;

public class ProjectMock : IProject
{
    public ProjectType Type => ProjectType.None;
    public string Name { get; private set; }
    public Version Version { get; private set; }
    public string Description { get; private set; }
    public string Directory { get; private set; }
    public string File { get; private set; }
    public HashSet<Dependency<IProject>> Projects => new();
    public HashSet<Dependency<Package>> Packages => new();

    public ProjectMock(
        string name,
        Version version,
        string description,
        string directory,
        string file
    )
    {
        Name = name;
        Version = version;
        Description = description;
        Directory = directory;
        File = file;
    }

    public bool IsRelated(string path) =>
        throw new NotImplementedException();

    public void Save() =>
        throw new NotImplementedException();

    public void SetDirectory(string directory) =>
        throw new NotImplementedException();

    public void SetName(string name) =>
        throw new NotImplementedException();

    public void SetVersion(Version version) =>
        throw new NotImplementedException();

    public override string ToString() => Name;
}