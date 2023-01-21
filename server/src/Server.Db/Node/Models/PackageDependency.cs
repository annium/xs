using Server.Db.Shared.Models;

namespace Server.Db.Node.Models;

public class PackageDependency : IPackageDependency
{
    public DependencyType Type { get; }

    public string Name { get; }

    public string Version { get; }

    public PackageDependency(
        DependencyType type,
        string name,
        string version
    )
    {
        Type = type;
        Name = name;
        Version = version;
    }
}