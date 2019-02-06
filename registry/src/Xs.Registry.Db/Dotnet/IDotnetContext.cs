using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db.Dotnet
{
    internal interface IDotnetContext : IContext
    {
        DbSet<Entities.Package> Packages { get; }

        DbSet<Entities.PackageDependency> PackageDependencies { get; }
    }
}