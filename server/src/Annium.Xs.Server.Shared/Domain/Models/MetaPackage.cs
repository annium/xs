using System;
using System.Collections.Generic;
using Annium.Data.Models;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using NodaTime;

namespace Annium.Xs.Server.Shared.Domain.Models;

public sealed record MetaPackage : IPackageInfo, IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public ProjectType Type { get; private init; } = default!;
    public string Name { get; private init; } = string.Empty;
    public string Version { get; private init; } = string.Empty;
    public string Description { get; private init; } = string.Empty;
    public Instant Published { get; private init; }
    public int Downloads { get; private init; }
    public Guid OwnerId { get; private init; }
    public User Owner { get; private init; } = default!;
    public IReadOnlyCollection<MetaPackagePermission> Permissions { get; private init; } =
        Array.Empty<MetaPackagePermission>();

    public MetaPackage(
        ProjectType type,
        string name,
        string version,
        string description,
        Instant published,
        int downloads,
        Guid ownerId,
        User owner,
        IReadOnlyCollection<MetaPackagePermission> permissions
    )
    {
        Id = Guid.NewGuid();
        Type = type;
        Name = name;
        Version = version;
        Description = description;
        Published = published;
        Downloads = downloads;
        OwnerId = ownerId;
        Owner = owner;
        Permissions = permissions;
    }

    internal MetaPackage() { }
}
