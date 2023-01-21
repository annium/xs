using Xs.Registry.Db.Shared.Models;

namespace Xs.Registry.Db.Shared.Tools;

public interface IMetaPackageManager
{
    MetaPackage Generate(User user, ProjectType type, IPackageInfo package);

    MetaPackageAccess GetAccess(MetaPackage metaPackage);
}