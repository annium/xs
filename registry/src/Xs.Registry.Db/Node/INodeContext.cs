using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db.Node
{
    internal interface INodeContext : IContext
    {
        DbSet<Entities.Package> NodePackages { get; }

        DbSet<Entities.PackageDependency> NodePackageDependencies { get; }
    }
}