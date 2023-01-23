using System.Collections.Generic;
using Server.Shared.Domain.Enums;
using Server.Shared.Domain.Interfaces;
using Server.Shared.Domain.Models;
using Server.Shared.Tools;

namespace Server.Shared.Internal.Tools;

internal class MetaPackageManager : IMetaPackageManager
{
    public MetaPackage Generate(User user, ProjectType type, IPackageInfo package)
    {
        var permissions = new List<MetaPackagePermission>();
        var metapackage = new MetaPackage(
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

        permissions.Add(new MetaPackagePermission(metapackage.Id, PermissionCategory.Owner, Permission.Read | Permission.Publish));
        permissions.Add(new MetaPackagePermission(metapackage.Id, PermissionCategory.World, Permission.None));

        return metapackage;
    }

    public MetaPackageAccess GetAccess(MetaPackage metaPackage) =>
        new(metaPackage.OwnerId, metaPackage.Permissions);
}