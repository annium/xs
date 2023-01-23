using System;
using Server.Shared.Domain.Enums;

namespace Server.Shared.Domain.Models;

public sealed record MetaPackagePermission
{
    public Guid MetaPackageId { get; private init; }
    public PermissionCategory Category { get; private init; }
    public Permission Permission { get; private init; }

    public MetaPackagePermission(
        Guid metaPackageId,
        PermissionCategory category,
        Permission permission
    )
    {
        MetaPackageId = metaPackageId;
        Category = category;
        Permission = permission;
    }

    internal MetaPackagePermission()
    {
    }
}