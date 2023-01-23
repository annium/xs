using System.Linq;
using Server.Abstractions.Services;
using Server.Domain.Models;
using Server.Node.Domain;
using Server.Node.Internal.Services;

namespace Server.Node.Views.Requests;

internal class PackageRequestParser : IPackageRequestParser<Package, PackageDependency, PackagePackageRequest>
{
    public Package Parse(MetaPackage metaPackage, PackagePackageRequest request)
    {
        var version = request.Versions[request.Version];

        return new Package(
            metaPackage.Id,
            request.Name,
            request.Version,
            request.Description,
            request.Published,
            version.Main,
            version.Distribution.Shasum,
            version.Distribution.Integrity,
            version.Dependencies.Select(d => new PackageDependency(DependencyType.Normal, d.Key, d.Value))
                .Concat(version.DevDependencies.Select(d => new PackageDependency(DependencyType.Dev, d.Key, d.Value)))
                .ToArray()
        );
    }
}