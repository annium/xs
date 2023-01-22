using System;
using Annium.Data.Models;
using Server.Domain.Enums;

namespace Server.Domain.Models;

public sealed record MetaPackagePermission : IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public Guid MetaPackageId { get; private init; }
    public PermissionCategory Category { get; private init; }
    public Permission Permission { get; private init; }

    public MetaPackagePermission(
        Guid metaPackageId,
        PermissionCategory category,
        Permission permission
    )
    {
        Id = Guid.NewGuid();
        MetaPackageId = metaPackageId;
        Category = category;
        Permission = permission;
    }

    internal MetaPackagePermission()
    {
    }
}