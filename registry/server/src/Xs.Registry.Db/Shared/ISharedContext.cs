using LinqToDB;

namespace Xs.Registry.Db.Shared
{
    internal interface ISharedContext : IContext
    {
        ITable<Entities.MetaPackage> MetaPackages { get; }

        ITable<Entities.MetaPackagePermission> MetaPackagePermissions { get; }

        ITable<Entities.User> Users { get; }

        ITable<Entities.UserSession> UserSessions { get; }
    }
}