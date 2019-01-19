using Xs.Core.Models;

namespace Xs.Registry.Core.Tools
{
    public interface IMetadataManager
    {
        PermissionCategory GetPermissionCategory(User user, Metadata metadata);

        Metadata Generate(User user, ProjectType projectType, string packageName);

        bool CheckPermission(User user, Metadata metadata, Permission permission);

        Metadata AddPermission(Metadata metadata, PermissionCategory category, Permission permission);

        Metadata DeletePermission(Metadata metadata, PermissionCategory category, Permission permission);
    }
}