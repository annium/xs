using Xs.Core.Models;

namespace Xs.Registry.Core.Tools
{
    public interface IMetadataManager
    {
        PermissionCategory GetPermissionCategory(User user, Metadata metadata);

        Metadata Generate(User user, ProjectType projectType, string packageName);

        bool CheckPermission(User user, Metadata metadata, Permission permission);

        Metadata GrantPermission(Metadata metadata, PermissionCategory category, Permission permission);

        Metadata RevokePermission(Metadata metadata, PermissionCategory category, Permission permission);
    }
}