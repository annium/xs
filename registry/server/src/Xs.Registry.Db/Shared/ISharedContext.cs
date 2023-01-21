using LinqToDB;
using Xs.Registry.Db.Shared.Entities;

namespace Xs.Registry.Db.Shared;

internal interface ISharedContext : IContext
{
    ITable<MetaPackage> MetaPackages { get; }

    ITable<MetaPackagePermission> MetaPackagePermissions { get; }

    ITable<User> Users { get; }

    ITable<UserSession> UserSessions { get; }
}