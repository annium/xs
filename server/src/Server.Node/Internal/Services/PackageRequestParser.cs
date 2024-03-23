using System;
using System.Linq;
using Server.Abstractions.Services;
using Server.Node.Domain;
using Server.Node.Views.Requests;
using Server.Shared.Domain.Models;

namespace Server.Node.Internal.Services;

internal class PackageRequestParser : IPackageRequestParser<Package, PackageDependency, PackageRequest>
{
    public Package Parse(MetaPackage metaPackage, PackageRequest request)
    {
        var version = request.Versions[request.Version];
        var packageId = Guid.NewGuid();
        var dependencies = version
            .Dependencies.Select(d => new PackageDependency(packageId, DependencyType.Normal, d.Key, d.Value))
            .Concat(
                version.DevDependencies.Select(d => new PackageDependency(
                    packageId,
                    DependencyType.Dev,
                    d.Key,
                    d.Value
                ))
            )
            .ToArray();

        return new Package(
            packageId,
            metaPackage,
            request.Name,
            request.Version,
            request.Description,
            request.Published,
            version.Main,
            version.Distribution.Shasum,
            version.Distribution.Integrity,
            dependencies
        );
    }
}
