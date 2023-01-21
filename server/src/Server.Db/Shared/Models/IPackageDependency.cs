namespace Xs.Registry.Db.Shared.Models;

public interface IPackageDependency
{
    string Name { get; }

    string Version { get; }
}