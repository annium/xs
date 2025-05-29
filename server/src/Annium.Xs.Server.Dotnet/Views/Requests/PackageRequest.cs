using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Dotnet.Domain;
using Annium.Xs.Server.Dotnet.Internal;
using Annium.Xs.Server.Shared.Domain.Models;
using NodaTime;

namespace Annium.Xs.Server.Dotnet.Views.Requests;

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
