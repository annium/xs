namespace Server.Domain.Models;

public interface IPackageDependency
{
    string Name { get; }

    string Version { get; }
}