using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Xs.Registry.Db.Node.Entities;

namespace Xs.Registry.Db
{
    internal partial class Context : Node.INodeContext
    {
        public DbSet<Package> NodePackagesSet { get; set; }

        public ITable<Package> NodePackages => NodePackagesSet.ToLinqToDBTable();

        public DbSet<PackageDependency> NodePackageDependenciesSet { get; set; }

        public ITable<PackageDependency> NodePackageDependencies => NodePackageDependenciesSet.ToLinqToDBTable();

        private void ConfigureNode(ModelBuilder builder)
        {
            builder.Entity<Package>()
                .HasAlternateKey(p => new { p.LowerName, p.Version });
            builder.Entity<Package>()
                .HasOne<Shared.Entities.MetaPackage>().WithMany().IsRequired()
                .HasForeignKey(p => p.MetaPackageId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PackageDependency>()
                .HasKey(p => new { p.PackageId, p.Name });
            builder.Entity<PackageDependency>()
                .HasOne<Package>().WithMany(p => p.Dependencies).IsRequired()
                .HasForeignKey(d => d.PackageId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}