using System.Collections.Generic;
using System.Linq;
using Xs.Core.Models;

namespace Xs.Registry.Core.Tools
{
    internal class MetadataManager : IMetadataManager
    {
        public PermissionCategory GetPermissionCategory(User user, Metadata metadata)
        {
            return metadata.UserId == user.Id ? PermissionCategory.Owner : PermissionCategory.World;
        }

        public Metadata Generate(User user, ProjectType projectType, string packageName)
        {
            var permissions = new Dictionary<PermissionCategory, Permission>();
            permissions[PermissionCategory.Owner] = Permission.Read | Permission.Publish;
            permissions[PermissionCategory.World] = Permission.Read;

            return new Metadata(user.Id, projectType, packageName, permissions);
        }

        public bool CheckPermission(User user, Metadata metadata, Permission permission)
        {
            var category = GetPermissionCategory(user, metadata);

            return metadata.Permissions[GetPermissionCategory(user, metadata)].HasFlag(permission);
        }

        public Metadata AddPermission(Metadata metadata, PermissionCategory category, Permission permission)
        {
            var permissions = metadata.Permissions.ToDictionary(
                e => e.Key,
                e => e.Key == category ? e.Value | permission : e.Value
            );

            return new Metadata(metadata.UserId, metadata.ProjectType, metadata.PackageName, permissions);
        }

        public Metadata DeletePermission(Metadata metadata, PermissionCategory category, Permission permission)
        {
            var permissions = metadata.Permissions.ToDictionary(
                e => e.Key,
                e => e.Key == category ? e.Value ^ permission : e.Value
            );

            return new Metadata(metadata.UserId, metadata.ProjectType, metadata.PackageName, permissions);
        }
    }
}