using System;
using System.Collections.Generic;
using System.Linq;
using Server.Domain.Enums;

namespace Server.Domain.Models;

public record struct UserMetaPackageAccess
{
    public bool IsOwner { get; }
    public bool IsWorld { get; }

    private readonly Permission _permission;

    internal UserMetaPackageAccess(
        Guid userId,
        Guid ownerId,
        IEnumerable<MetaPackagePermission> permissions
    )
    {
        var category = ownerId == userId ? PermissionCategory.Owner : PermissionCategory.World;

        IsOwner = category == PermissionCategory.Owner;
        IsWorld = category == PermissionCategory.World;

        _permission = permissions.FirstOrDefault(p => p.Category == category)?.Permission ?? Permission.None;
    }

    public bool Has(Permission permission) => _permission.HasFlag(permission);
    public override int GetHashCode() => HashCode.Combine(IsOwner, IsWorld, _permission);
}