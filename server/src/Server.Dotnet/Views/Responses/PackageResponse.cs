using System;
using System.Collections.Generic;
using NodaTime;
using Server.Dotnet.Domain;

namespace Server.Dotnet.Views.Responses;

internal sealed record PackageResponse(
    Guid Id,
    string Name,
    string Version,
    string Description,
    Instant Published,
    int Downloads,
    IReadOnlyCollection<PackageDependency> Dependencies
);