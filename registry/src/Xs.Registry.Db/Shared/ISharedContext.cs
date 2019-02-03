using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db.Shared
{
    internal interface ISharedContext
    {
        DbSet<Entities.MetaPackage> MetaPackages { get; }

        DbSet<Entities.MetaPackagePermission> MetaPackagePermissions { get; }

        DbSet<Entities.User> Users { get; }

        DbSet<Entities.UserSession> UserSessions { get; }
    }
}