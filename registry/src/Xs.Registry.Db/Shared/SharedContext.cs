using Microsoft.EntityFrameworkCore;
using Xs.Registry.Db.Shared.Entities;

namespace Xs.Registry.Db
{
    internal partial class Context : Shared.ISharedContext
    {
        public virtual DbSet<MetaPackage> MetaPackages { get; set; }

        public virtual DbSet<MetaPackagePermission> MetaPackagePermissions { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserSession> UserSessions { get; set; }

        private void ConfigureShared(ModelBuilder builder)
        {
            builder.Entity<MetaPackage>()
                .Property(m => m.Type)
                .HasConversion(t => t.ToString(), t => Shared.ProjectType.Get(t));
            builder.Entity<MetaPackage>()
                .HasOne<User>(m => m.Owner).WithMany().IsRequired()
                .HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);

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
}