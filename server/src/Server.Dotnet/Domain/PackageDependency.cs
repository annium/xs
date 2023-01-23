using System;
using Annium.Data.Models;
using Server.Domain.Interfaces;

namespace Server.Dotnet.Domain;

public class PackageDependency : IPackageDependency, IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public Guid PackageId { get; private init; }
    public string Framework { get; private init; } = string.Empty;
    public string Name { get; private init; } = string.Empty;
    public string Version { get; private init; } = string.Empty;

    public PackageDependency(
        Guid packageId,
        string framework,
        string name,
        string version
    )
    {
        Id = Guid.NewGuid();
        PackageId = packageId;
        Framework = framework;
        Name = name;
        Version = version;
    }

    internal PackageDependency()
    {
    }
}