using System;
using System.Collections.Generic;
using Annium.Data.Models;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;
using NodaTime;

namespace Annium.Xs.Server.Node.Domain;

public class Package : IPackage<PackageDependency>, IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public Guid MetaPackageId { get; private init; }
    public MetaPackage MetaPackage { get; private init; } = default!;
    public string Name { get; private init; } = string.Empty;
    public string Version { get; private init; } = string.Empty;
    public string Description { get; private init; } = string.Empty;
    public Instant Published { get; private init; }
    public int Downloads { get; private init; }
    public string Main { get; private init; } = string.Empty;
    public string Shasum { get; private init; } = string.Empty;
    public string Integrity { get; private init; } = string.Empty;
    public IReadOnlyCollection<PackageDependency> Dependencies { get; private init; } =
        Array.Empty<PackageDependency>();

    public Package(
        Guid id,
        MetaPackage metaPackage,
        string name,
        string version,
        string description,
        Instant published,
        string main,
        string shasum,
        string integrity,
        IReadOnlyCollection<PackageDependency> dependencies
    )
    {
        Id = id;
        MetaPackageId = metaPackage.Id;
        Name = name;
        Version = version;
        Description = description;
        Published = published;
        Main = main;
        Shasum = shasum;
        Integrity = integrity;
        Dependencies = dependencies;
    }

    internal Package() { }
}
