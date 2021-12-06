using LinqToDB;

namespace Xs.Registry.Db.Node;

internal interface INodeContext : IContext
{
    ITable<Entities.Package> NodePackages { get; }

    ITable<Entities.PackageDependency> NodePackageDependencies { get; }
}