using System;
using System.Collections.Generic;
using System.Linq;

namespace Xs.Registry.Db.Shared
{
    public struct UserMetaPackageAccess
    {
        public bool IsOwner { get; }

        public bool IsWorld { get; }

        private readonly Permission permission;

        internal UserMetaPackageAccess(
            Guid userId,
            Guid ownerId,
            IEnumerable<MetaPackagePermission> permissions
        )
        {
            var category = ownerId == userId ? PermissionCategory.Owner : PermissionCategory.World;

            IsOwner = category == PermissionCategory.Owner;
            IsWorld = category == PermissionCategory.World;

            permission = permissions.FirstOrDefault(p => p.Category == category)?.Permission ?? Permission.None;
        }

        public bool Has(Permission permission) => this.permission.HasFlag(permission);
    }
}