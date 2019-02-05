using System.Collections.Generic;

using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    public interface IMetaPackageManager
    {
        MetaPackage Generate(User user, ProjectType type, IPackageBase package);

        PermissionCategory GetPermissionCategory(User user, MetaPackage metaPackage);

        bool CheckPermission(User user, MetaPackage metaPackage, Permission permission);

        void Update(MetaPackage metaPackage, IPackageBase package);

        void SetPermissions(MetaPackage metaPackage, IEnumerable<MetaPackagePermission> permissions);
    }
}