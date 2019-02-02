using System.Collections.Generic;
using Xs.Core.Models;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    public interface IMetaPackageManager
    {
        PermissionCategory GetPermissionCategory(User user, MetaPackage metaPackage);

        MetaPackage Generate(User user);

        bool CheckPermission(User user, MetaPackage metaPackage, Permission permission);

        MetaPackage SetPermissions(
            MetaPackage metaPackage,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        );
    }
}