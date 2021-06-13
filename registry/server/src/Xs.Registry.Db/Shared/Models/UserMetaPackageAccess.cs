using System;
using System.Collections.Generic;
using System.Linq;

namespace Xs.Registry.Db.Shared
{
    public struct UserMetaPackageAccess
    {
        public static readonly UserMetaPackageAccess None =
            new(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<MetaPackagePermission>());

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

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 7;

                hash = hash * 31 + IsOwner.GetHashCode();
                hash = hash * 31 + IsWorld.GetHashCode();
                hash = hash * 31 + _permission.GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj) => GetType() == obj?.GetType() && GetHashCode() == obj.GetHashCode();

        public static bool operator ==(UserMetaPackageAccess a, UserMetaPackageAccess b) => a.GetHashCode() == b.GetHashCode();

        public static bool operator !=(UserMetaPackageAccess a, UserMetaPackageAccess b) => !(a == b);
    }
}