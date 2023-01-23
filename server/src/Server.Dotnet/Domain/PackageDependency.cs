using System;
using Server.Shared.Domain.Interfaces;

namespace Server.Dotnet.Domain;

public class PackageDependency : IPackageDependency
{
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
        PackageId = packageId;
        Framework = framework;
        Name = name;
        Version = version;
    }

    internal PackageDependency()
    {
    }
}