using System.Collections.Generic;

namespace Xs.Registry.Db.Shared
{
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
            new MetaPackageAccess(metaPackage.OwnerId, metaPackage.Permissions);
    }
}