using LinqToDB;

namespace Xs.Registry.Db.Dotnet
{
    internal interface IDotnetContext : IContext
    {
        ITable<Entities.Package> DotnetPackages { get; }

        ITable<Entities.PackageDependency> DotnetPackageDependencies { get; }
    }
}