using LinqToDB;
using Xs.Registry.Db.Dotnet.Entities;

namespace Xs.Registry.Db.Dotnet;

internal interface IDotnetContext : IContext
{
    ITable<Package> DotnetPackages { get; }

    ITable<PackageDependency> DotnetPackageDependencies { get; }
}