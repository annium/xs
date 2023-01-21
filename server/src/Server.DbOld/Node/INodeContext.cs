using LinqToDB;
using Server.Db.Node.Entities;

namespace Server.Db.Node;

internal interface INodeContext : IContext
{
    ITable<Package> NodePackages { get; }

    ITable<PackageDependency> NodePackageDependencies { get; }
}