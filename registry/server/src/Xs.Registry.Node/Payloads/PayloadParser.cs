using System;
using System.Linq;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Node;

namespace Xs.Registry.Node.Payloads
{
    internal class PayloadParser : IPayloadParser<PackagePayload, Package, PackageDependency>
    {
        public Package Parse(Guid metaPackageId, PackagePayload payload)
        {
            var version = payload.Versions[payload.Version];

            return new Package(
                metaPackageId,
                payload.Name,
                payload.Version,
                payload.Description,
                payload.Published,
                version.Main,
                version.Distribution.Shasum,
                version.Distribution.Integrity,
                version.Dependencies.Select(d => new PackageDependency(DependencyType.Normal, d.Key, d.Value))
                .Concat(version.DevDependencies.Select(d => new PackageDependency(DependencyType.Dev, d.Key, d.Value)))
                .ToArray()
            );
        }
    }
}