using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db.Dotnet
{
    internal interface IDotnetContext : IContext
    {
        DbSet<Entities.Package> DotnetPackages { get; }

        DbSet<Entities.PackageDependency> DotnetPackageDependencies { get; }
    }
}