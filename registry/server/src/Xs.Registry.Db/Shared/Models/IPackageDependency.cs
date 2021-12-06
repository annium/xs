namespace Xs.Registry.Db.Shared;

public interface IPackageDependency
{
    string Name { get; }

    string Version { get; }
}