using Microsoft.EntityFrameworkCore;
using Xs.Registry.Db.Dotnet.Entities;

namespace Xs.Registry.Db
{
    internal partial class Context : Dotnet.IDotnetContext
    {
        public DbSet<Package> Packages { get; set; }

        public DbSet<PackageDependency> PackageDependencies { get; set; }

        private void ConfigureDotnet(ModelBuilder builder)
        {
            builder.Entity<Package>()
                .HasAlternateKey(p => new { p.LowerName, p.Version });
            builder.Entity<Package>()
                .HasOne<Shared.Entities.MetaPackage>().WithMany().IsRequired()
                .HasForeignKey(p => p.MetaPackageId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PackageDependency>()
                .HasKey(p => new { p.PackageId, p.Framework, p.Name });
            builder.Entity<PackageDependency>()
                .HasOne<Package>().WithMany(p => p.Dependencies).IsRequired()
                .HasForeignKey(d => d.PackageId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}