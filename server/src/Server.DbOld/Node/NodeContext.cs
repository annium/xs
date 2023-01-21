using LinqToDB;
using Microsoft.EntityFrameworkCore;
using Server.Db.Node;
using Server.Db.Node.Entities;
using Server.Db.Shared.Entities;

namespace Server.Db;

internal partial class Context : INodeContext
{
    public DbSet<Package> NodePackagesSet { get; set; }

    public ITable<Package> NodePackages => Table(NodePackagesSet);

    public DbSet<PackageDependency> NodePackageDependenciesSet { get; set; }

    public ITable<PackageDependency> NodePackageDependencies => Table(NodePackageDependenciesSet);

    private void ConfigureNode(ModelBuilder builder)
    {
        builder.Entity<Package>()
            .HasAlternateKey(p => new { p.LowerName, p.Version });
        builder.Entity<Package>()
            .HasOne<MetaPackage>().WithMany().IsRequired()
            .HasForeignKey(p => p.MetaPackageId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PackageDependency>()
            .HasKey(p => new { p.PackageId, p.Name });
        builder.Entity<PackageDependency>()
            .HasOne<Package>().WithMany(p => p.Dependencies).IsRequired()
            .HasForeignKey(d => d.PackageId).OnDelete(DeleteBehavior.Cascade);
    }
}