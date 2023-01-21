using LinqToDB;
using Server.Db.Shared.Entities;

namespace Server.Db.Shared;

internal interface ISharedContext : IContext
{
    ITable<MetaPackage> MetaPackages { get; }

    ITable<MetaPackagePermission> MetaPackagePermissions { get; }

    ITable<User> Users { get; }

    ITable<UserSession> UserSessions { get; }
}