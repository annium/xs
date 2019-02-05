using Microsoft.EntityFrameworkCore;

using Xs.Registry.Db.Dotnet;

namespace Xs.Registry.Db
{
    internal class Context : DbContext, ISharedContext, IDotnetContext
    {
        public virtual DbSet<Models.MetaPackage> MetaPackages { get; set; }

        public virtual DbSet<Models.MetaPackagePermission> MetaPackagePermissions { get; set; }

        public virtual DbSet<Models.User> Users { get; set; }

        public virtual DbSet<Models.UserSession> UserSessions { get; set; }

        public Context(DbContextOptions<Context> contextOptions) : base(contextOptions) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Models.MetaPackage>()
                .Property(m => m.Type)
                .HasConversion(t => t.ToString(), t => ProjectType.Get(t));
            builder.Entity<Models.MetaPackage>()
                .HasOne<Models.User>(m => m.Owner).WithMany().IsRequired()
                .HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Models.MetaPackagePermission>()
                .HasKey(p => new { p.MetaPackageId, p.Category });
            builder.Entity<Models.MetaPackagePermission>()
                .HasOne<Models.MetaPackage>().WithMany(m => m.Permissions).IsRequired()
                .HasForeignKey(m => m.MetaPackageId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Models.UserSession>()
                .HasOne<Models.User>().WithMany().IsRequired()
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}