using System;
using Annium.Data.Models;
using Server.Domain.Interfaces;

namespace Server.Node.Domain;

public class PackageDependency : IPackageDependency, IIdEntity<Guid>
{
    public Guid Id { get; private init; }
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
        Id = Guid.NewGuid();
        PackageId = packageId;
        Type = type;
        Name = name;
        Version = version;
    }

    internal PackageDependency()
    {
    }
}