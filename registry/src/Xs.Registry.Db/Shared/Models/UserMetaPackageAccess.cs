using System.Linq;

namespace Xs.Registry.Db.Shared
{
    public struct UserMetaPackageAccess
    {
        public bool IsOwner { get; }

        public bool IsWorld { get; }

        private readonly Permission permission;

        internal UserMetaPackageAccess(
            User user,
            MetaPackage metaPackage
        )
        {
            var category = metaPackage.OwnerId == user.Id ? PermissionCategory.Owner : PermissionCategory.World;

            IsOwner = category == PermissionCategory.Owner;
            IsWorld = category == PermissionCategory.World;

            permission = metaPackage.Permissions.FirstOrDefault(p => p.Category == category)?.Permission ?? Permission.None;
        }

        public bool Has(Permission permission) => this.permission.HasFlag(permission);
    }
}