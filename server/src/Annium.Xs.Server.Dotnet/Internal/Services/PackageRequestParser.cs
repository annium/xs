using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Dotnet.Domain;
using Annium.Xs.Server.Dotnet.Views.Requests;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Dotnet.Internal.Services;

internal class PackageRequestParser : IPackageRequestParser<Package, PackageDependency, PackageRequest>
{
    public Package Parse(MetaPackage metaPackage, PackageRequest request)
    {
        return new Package(
            request.Id,
            metaPackage,
            request.Name,
            request.Version,
            request.Description,
            request.Published,
            request.Dependencies
        );
    }
}
