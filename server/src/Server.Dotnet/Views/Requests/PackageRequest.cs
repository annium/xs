using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using NodaTime;
using Server.Abstractions.Domain;
using Server.Domain.Models;
using Server.Dotnet.Domain;

namespace Server.Dotnet.Views.Requests;

public sealed record PackageRequest(
    Guid Id,
    string Name,
    string Version,
    string Description,
    Instant Published,
    IReadOnlyCollection<PackageDependency> Dependencies,
    Stream Stream,
    Stream NuspecStream
) : IPackageRequest
{
    [JsonIgnore]
    public ProjectType ProjectType => Constants.ProjectType;
}