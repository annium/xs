using System.Collections.Generic;
using System.Linq;

using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    internal class MetaPackageManager : IMetaPackageManager
    {
        public MetaPackage Generate(User user, ProjectType type, IPackageBase package)
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

        public PermissionCategory GetPermissionCategory(User user, MetaPackage metaPackage)
        {
            return metaPackage.OwnerId == user.Id ? PermissionCategory.Owner : PermissionCategory.World;
        }

        public bool CheckPermission(User user, MetaPackage metaPackage, Permission permission)
        {
            var category = GetPermissionCategory(user, metaPackage);

            return metaPackage.Permissions.Any(p => p.Category == category && p.Permission.HasFlag(permission));
        }

        public void Update(MetaPackage metaPackage, IPackageBase package)
        {
            metaPackage.Name = package.Name;
            metaPackage.Version = package.Version;
            metaPackage.Description = package.Description;
            metaPackage.Published = package.Published;
        }

        public void SetPermissions(MetaPackage metaPackage, IEnumerable<MetaPackagePermission> permissions)
        {
            metaPackage.Permissions = permissions;
        }
    }
}