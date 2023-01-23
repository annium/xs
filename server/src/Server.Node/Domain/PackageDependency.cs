using System;
using Server.Shared.Domain.Interfaces;

namespace Server.Node.Domain;

public class PackageDependency : IPackageDependency
{
    public Guid PackageId { get; private init; }
    public DependencyType Type { get; private init; }
    public string Name { get; private init; } = string.Empty;
    public string Version { get; private init; } = string.Empty;

    public PackageDependency(
        Guid packageId,
        DependencyType type,
        string name,
        string version
    )
    {
        PackageId = packageId;
        Type = type;
        Name = name;
        Version = version;
    }

    internal PackageDependency()
    {
    }
}