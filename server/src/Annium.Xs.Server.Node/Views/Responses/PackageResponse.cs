using System;
using System.Collections.Generic;
using Annium.Xs.Server.Node.Domain;
using NodaTime;

namespace Annium.Xs.Server.Node.Views.Responses;

internal sealed record PackageResponse
{
    public Guid Id { get; }

    public string Name { get; }

    public string Version { get; }

    public string Description { get; }

    public Instant Published { get; }

    public int Downloads { get; }

    public string Main { get; }

    public string Shasum { get; }

    public string Integrity { get; }

    public IReadOnlyCollection<PackageDependency> Dependencies { get; }

    internal PackageResponse(Package package)
    {
        Id = package.Id;
        Name = package.Name;
        Version = package.Version;
        Description = package.Description;
        Published = package.Published;
        Downloads = package.Downloads;
        Main = package.Main;
        Shasum = package.Shasum;
        Integrity = package.Integrity;
        Dependencies = package.Dependencies;
    }
}
