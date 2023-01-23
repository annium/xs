using System;
using System.Collections.Generic;
using NodaTime;
using Server.Domain.Models;

namespace Server.Main.Views.Responses;

internal sealed record MetaPackageResponse
{
    public Guid Id { get; }
    public string Type { get; }
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public Instant Published { get; }
    public int Downloads { get; }
    public Guid OwnerId { get; }
    public string Owner { get; }
    public IReadOnlyCollection<MetaPackagePermission> Permissions { get; }

    internal MetaPackageResponse(MetaPackage metaPackage)
    {
        Id = metaPackage.Id;
        Type = metaPackage.Type.ToString();
        Name = metaPackage.Name;
        Version = metaPackage.Version;
        Description = metaPackage.Description;
        Published = metaPackage.Published;
        Downloads = metaPackage.Downloads;
        OwnerId = metaPackage.OwnerId;
        Owner = metaPackage.Owner.Login;
        Permissions = metaPackage.Permissions;
    }
}