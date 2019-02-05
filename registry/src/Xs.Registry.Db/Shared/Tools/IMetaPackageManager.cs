namespace Xs.Registry.Db.Shared
{
    public interface IMetaPackageManager
    {
        MetaPackage Generate(User user, ProjectType type, IPackageInfo package);

        UserMetaPackageAccess GetAccess(User user, MetaPackage metaPackage);
    }
}