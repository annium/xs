namespace Xs.Registry.Db.Shared;

public interface IMetaPackageManager
{
    MetaPackage Generate(User user, ProjectType type, IPackageInfo package);

    MetaPackageAccess GetAccess(MetaPackage metaPackage);
}