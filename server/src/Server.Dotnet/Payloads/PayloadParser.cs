using System;
using Server.Abstractions.Packages;

namespace Server.Dotnet.Payloads;

internal class PayloadParser : IPayloadParser<PackagePayload, Package, PackageDependency>
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