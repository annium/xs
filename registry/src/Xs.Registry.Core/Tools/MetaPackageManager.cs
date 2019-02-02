using System.Collections.Generic;
using Xs.Core.Models;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    internal class MetaPackageManager : IMetaPackageManager
    {
        public PermissionCategory GetPermissionCategory(User user, MetaPackage metaPackage)
        {
            return metaPackage.OwnerId == user.Id ? PermissionCategory.Owner : PermissionCategory.World;
        }

        public MetaPackage Generate(User user)
        {
            var permissions = new Dictionary<PermissionCategory, Permission>();
            permissions[PermissionCategory.Owner] = Permission.Read | Permission.Publish;
            permissions[PermissionCategory.World] = Permission.Read;

            return new MetaPackage(user.Id, permissions);
        }

        public bool CheckPermission(User user, MetaPackage metaPackage, Permission permission)
        {
            var category = GetPermissionCategory(user, metaPackage);

            return metaPackage.Permissions[GetPermissionCategory(user, metaPackage)].HasFlag(permission);
        }

        public MetaPackage SetPermissions(
            MetaPackage metaPackage,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        )
        {
            return new MetaPackage(metaPackage.OwnerId, permissions);
        }
    }
}