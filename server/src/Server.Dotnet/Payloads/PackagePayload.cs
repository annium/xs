using System.Collections.Generic;
using System.IO;
using NodaTime;
using Server.Abstractions.Packages;
using Xs.Registry.Db.Dotnet.Models;

namespace Server.Dotnet.Payloads;

public class PackagePayload : IPayload
{
    public string Name { get; }

    public string Version { get; }

    public string Description { get; }

    public Instant Published { get; }

    public IEnumerable<PackageDependency> Dependencies { get; }

    public Stream Stream { get; }

    internal PackagePayload(
        string name,
        string version,
        string description,
        Instant published,
        IEnumerable<PackageDependency> dependencies,
        Stream stream,
        Stream nuspecStream
    )
    {
        Name = name;
        Version = version;
        Description = description;
        Published = published;
        Dependencies = dependencies;
        Stream = stream;
    }
}