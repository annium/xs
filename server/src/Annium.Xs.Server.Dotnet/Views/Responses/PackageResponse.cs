using System;
using System.Collections.Generic;
using Annium.Xs.Server.Dotnet.Domain;
using NodaTime;

namespace Annium.Xs.Server.Dotnet.Views.Responses;

internal sealed record PackageResponse(
    Guid Id,
    string Name,
    string Version,
    string Description,
    Instant Published,
    int Downloads,
    IReadOnlyCollection<PackageDependency> Dependencies
);
