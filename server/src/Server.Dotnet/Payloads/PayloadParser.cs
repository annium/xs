using System;
using Server.Abstractions.Services;
using Server.Dotnet.Domain;

namespace Server.Dotnet.Payloads;

internal class PayloadParser : IPayloadParser<Package, PackageDependency, PackagePayload>
{
    public Package Parse(Guid metaPackageId, PackagePayload payload)
    {
        return new(
            metaPackageId,
            payload.Name,
            payload.Version,
            payload.Description,
            payload.Published,
            payload.Dependencies
        );
    }
}