using LinqToDB;
using Server.Db.Dotnet.Entities;

namespace Server.Db.Dotnet;

internal interface IDotnetContext : IContext
{
    ITable<Package> DotnetPackages { get; }

    ITable<PackageDependency> DotnetPackageDependencies { get; }
}