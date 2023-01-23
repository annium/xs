using System;
using System.Collections.Generic;
using Annium.Data.Models;
using NodaTime;
using Server.Domain.Interfaces;
using Server.Domain.Models;

namespace Server.Dotnet.Domain;

public sealed record Package : IPackage<PackageDependency>, IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public Guid MetaPackageId { get; private init; }
    public MetaPackage MetaPackage { get; private init; } = default!;
    public string Name { get; private init; } = string.Empty;
    public string Version { get; private init; } = string.Empty;
    public string Description { get; private init; } = string.Empty;
    public Instant Published { get; private init; }
    public int Downloads { get; private init; }
    public IReadOnlyCollection<PackageDependency> Dependencies { get; private init; } = Array.Empty<PackageDependency>();

    public Package(
        Guid id,
        MetaPackage metaPackage,
        string name,
        string version,
        string description,
        Instant published,
        IReadOnlyCollection<PackageDependency> dependencies
    )
    {
        Id = id;
        MetaPackageId = metaPackage.Id;
        MetaPackage = metaPackage;
        Name = name;
        Version = version;
        Description = description;
        Published = published;
        Dependencies = dependencies;
    }

    internal Package()
    {
    }
}