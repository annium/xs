using System;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Dotnet.Models;

namespace Xs.Registry.Dotnet.Payloads;

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