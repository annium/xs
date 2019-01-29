using System.Collections.Generic;
using Xs.Core.Models;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    internal class MetadataManager : IMetadataManager
    {
        public PermissionCategory GetPermissionCategory(User user, Metadata metadata)
        {
            return metadata.OwnerId == user.Id ? PermissionCategory.Owner : PermissionCategory.World;
        }

        public Metadata Generate(User user)
        {
            var permissions = new Dictionary<PermissionCategory, Permission>();
            permissions[PermissionCategory.Owner] = Permission.Read | Permission.Publish;
            permissions[PermissionCategory.World] = Permission.Read;

            return new Metadata(user.Id, permissions);
        }

        public bool CheckPermission(User user, Metadata metadata, Permission permission)
        {
            var category = GetPermissionCategory(user, metadata);

            return metadata.Permissions[GetPermissionCategory(user, metadata)].HasFlag(permission);
        }

        public Metadata SetPermissions(
            Metadata metadata,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        )
        {
            return new Metadata(metadata.OwnerId, permissions);
        }
    }
}