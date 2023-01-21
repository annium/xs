using LinqToDB;
using Microsoft.EntityFrameworkCore;
using Server.Db.Dotnet;
using Server.Db.Dotnet.Entities;
using Server.Db.Shared.Entities;

namespace Server.Db;

internal partial class Context : IDotnetContext
{
    public DbSet<Package> DotnetPackagesSet { get; set; }

    public ITable<Package> DotnetPackages => Table(DotnetPackagesSet);

    public DbSet<PackageDependency> DotnetPackageDependenciesSet { get; set; }

    public ITable<PackageDependency> DotnetPackageDependencies => Table(DotnetPackageDependenciesSet);

    private void ConfigureDotnet(ModelBuilder builder)
    {
        builder.Entity<Package>()
            .HasAlternateKey(p => new { p.LowerName, p.Version });
        builder.Entity<Package>()
            .HasOne<MetaPackage>().WithMany().IsRequired()
            .HasForeignKey(p => p.MetaPackageId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PackageDependency>()
            .HasKey(p => new { p.PackageId, p.Framework, p.Name });
        builder.Entity<PackageDependency>()
            .HasOne<Package>().WithMany(p => p.Dependencies).IsRequired()
            .HasForeignKey(d => d.PackageId).OnDelete(DeleteBehavior.Cascade);
    }
}