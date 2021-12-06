using LinqToDB;
using Microsoft.EntityFrameworkCore;
using Xs.Registry.Db.Shared.Entities;

namespace Xs.Registry.Db;

internal partial class Context : Shared.ISharedContext
{
    public virtual DbSet<MetaPackage> MetaPackagesSet { get; set; }

    public ITable<MetaPackage> MetaPackages => Table(MetaPackagesSet);

    public virtual DbSet<MetaPackagePermission> MetaPackagePermissionsSet { get; set; }

    public ITable<MetaPackagePermission> MetaPackagePermissions => Table(MetaPackagePermissionsSet);

    public virtual DbSet<User> UsersSet { get; set; }

    public ITable<User> Users => Table(UsersSet);

    public virtual DbSet<UserSession> UserSessionsSet { get; set; }

    public ITable<UserSession> UserSessions => Table(UserSessionsSet);

    private void ConfigureShared(ModelBuilder builder)
    {
        builder.Entity<MetaPackage>()
            .HasOne<User>(m => m.Owner).WithMany().IsRequired()
            .HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<MetaPackage>()
            .HasAlternateKey(m => new { m.Type, m.LowerName });

        builder.Entity<MetaPackagePermission>()
            .HasKey(p => new { p.MetaPackageId, p.Category });
        builder.Entity<MetaPackagePermission>()
            .HasOne<MetaPackage>().WithMany(m => m.Permissions).IsRequired()
            .HasForeignKey(m => m.MetaPackageId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserSession>()
            .HasOne<User>().WithMany().IsRequired()
            .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}