using Server.Domain.Interfaces;

namespace Server.Dotnet.Domain;

public class PackageDependency : IPackageDependency
{
    public string Framework { get; }

    public string Name { get; }

    public string Version { get; }

    public PackageDependency(
        string framework,
        string name,
        string version
    )
    {
        Framework = framework;
        Name = name;
        Version = version;
    }
}