using System;
using Server.Abstractions.Services;
using Server.Dotnet.Domain;
using Server.Dotnet.Views.Requests;
using Server.Shared.Domain.Models;

namespace Server.Dotnet.Internal.Services;

internal class PackageRequestParser : IPackageRequestParser<Package, PackageDependency, PackageRequest>
{
    public Package Parse(MetaPackage metaPackage, PackageRequest request)
    {
        return new Package(
            Guid.NewGuid(), 
            metaPackage,
            request.Name,
            request.Version,
            request.Description,
            request.Published,
            request.Dependencies
        );
    }
}