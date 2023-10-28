using System;

namespace Xs.Cli.Core.Models;

public sealed record Package : IReference
{
    public ProjectType Type { get; }
    public string Name { get; }
    public Version Version { get; }

    public Package(ProjectType type, string name, Version version)
    {
        Type = type;
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        Name = name;
        Version = version ?? throw new ArgumentNullException(nameof(version));
    }

    public void Deconstruct(out ProjectType type, out string name, out Version version)
    {
        type = Type;
        name = Name;
        version = Version;
    }

    public override string ToString() => $"{Name} {Version}";

    public override int GetHashCode() => HashCode.Combine(Type, Name, Version);
}
