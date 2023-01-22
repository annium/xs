using System;
using System.Collections.Generic;
using NodaTime;
using Server.Domain.Interfaces;

namespace Server.Db.Node.Models;

public class Package : IPackage<PackageDependency>
{
    public Guid Id { get; }

    public Guid MetaPackageId { get; }

    public string Name { get; }

    public string Version { get; }

    public string Description { get; }

    public Instant Published { get; }

    public int Downloads { get; }

    public string Main { get; }

    public string Shasum { get; }

    public string Integrity { get; }

    public IEnumerable<PackageDependency> Dependencies { get; }

    public Package(
        Guid metaPackageId,
        string name,
        string version,
        string description,
        Instant published,
        string main,
        string shasum,
        string integrity,
        IEnumerable<PackageDependency> dependencies
    )
    {
        MetaPackageId = metaPackageId;
        Name = name;
        Version = version;
        Description = description;
        Published = published;
        Main = main;
        Shasum = shasum;
        Integrity = integrity;
        Dependencies = dependencies;
    }

    internal Package(
        Guid id,
        Guid metaPackageId,
        string name,
        string version,
        string description,
        Instant published,
        int downloads,
        string main,
        string shasum,
        string integrity,
        IEnumerable<PackageDependency> dependencies
    ) : this(metaPackageId, name, version, description, published, main, shasum, integrity, dependencies)
    {
        Id = id;
        Downloads = downloads;
    }
}