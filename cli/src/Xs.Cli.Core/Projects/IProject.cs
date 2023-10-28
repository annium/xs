using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

public interface IProject : IReference
{
    string Description { get; }
    string Directory { get; }
    string File { get; }
    HashSet<Dependency<IProject>> Projects { get; }
    HashSet<Dependency<Package>> Packages { get; }

    void SetDirectory(string directory);

    void SetName(string name);

    void SetVersion(Version version);

    bool IsRelated(string path);

    void Save();
}
