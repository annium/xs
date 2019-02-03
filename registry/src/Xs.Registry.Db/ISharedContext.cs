using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db
{
    public interface ISharedContext
    {
        DbSet<Models.MetaPackage> MetaPackages { get; }

        DbSet<Models.MetaPackagePermission> MetaPackagePermissions { get; }

        DbSet<Models.User> Users { get; }

        DbSet<Models.UserSession> UserSessions { get; }
    }
}