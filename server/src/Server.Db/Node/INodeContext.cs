using LinqToDB;
using Xs.Registry.Db.Node.Entities;

namespace Xs.Registry.Db.Node;

internal interface INodeContext : IContext
{
    ITable<Package> NodePackages { get; }

    ITable<PackageDependency> NodePackageDependencies { get; }
}