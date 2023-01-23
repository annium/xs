namespace Server.Shared.Domain.Interfaces;

public interface IPackageDependency
{
    string Name { get; }

    string Version { get; }
}