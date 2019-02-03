using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Dotnet.Db
{
    internal class DotnetDbContext : DbContext
    {
        internal const string Schema = "dotnet";

        public virtual DbSet<Models.Package> Packages { get; set; }

        public virtual DbSet<Models.PackageDependency> PackageDependencies { get; set; }

        public DotnetDbContext(DbContextOptions<DotnetDbContext> contextOptions) : base(contextOptions) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Models.Package>()
                .HasOne<Core.Db.Models.MetaPackage>(p => p.MetaPackage).WithMany().IsRequired()
                .HasForeignKey(p => p.MetaPackageId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Models.PackageDependency>()
                .HasOne<Models.Package>().WithMany().IsRequired()
                .HasForeignKey(d => d.PackageId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}