using Server.Domain.Enums;

namespace Server.Domain.Models;

public sealed record MetaPackagePermission
{
    public PermissionCategory Category { get; }

    public Permission Permission { get; }

    public MetaPackagePermission(
        PermissionCategory category,
        Permission permission
    )
    {
        Category = category;
        Permission = permission;
    }
}