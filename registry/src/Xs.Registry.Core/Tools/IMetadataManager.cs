using System.Collections.Generic;
using Xs.Core.Models;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    public interface IMetadataManager
    {
        PermissionCategory GetPermissionCategory(User user, Metadata metadata);

        Metadata Generate(User user);

        bool CheckPermission(User user, Metadata metadata, Permission permission);

        Metadata SetPermissions(
            Metadata metadata,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        );
    }
}