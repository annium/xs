using System.Collections.Generic;
using Server.Domain.Enums;
using Server.Domain.Interfaces;
using Server.Domain.Models;
using Server.Shared.Tools;

namespace Server.Shared.Internal.Tools;

internal class MetaPackageManager : IMetaPackageManager
{
    public MetaPackage Generate(User user, ProjectType type, IPackageInfo package)
    {
        var permissions = new List<MetaPackagePermission>();
        permissions.Add(new MetaPackagePermission(PermissionCategory.Owner, Permission.Read | Permission.Publish));
        permissions.Add(new MetaPackagePermission(PermissionCategory.World, Permission.None));

        return new MetaPackage(
            type,
            package.Name,
            package.Version,
            package.Description,
            package.Published,
            0,
            user.Id,
            user,
            permissions
        );
    }

    public MetaPackageAccess GetAccess(MetaPackage metaPackage) =>
        new(metaPackage.OwnerId, metaPackage.Permissions);
}