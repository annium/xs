using Server.Abstractions.Domain;
using Server.Shared.Domain.Interfaces;
using Server.Shared.Domain.Models;

namespace Server.Abstractions.Services;

public interface IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>
    where TPackage : IPackage<TPackageDependency>
    where TPackageDependency : IPackageDependency
    where TPackageRequest : IPackageRequest
{
    TPackage Parse(MetaPackage metaPackage, TPackageRequest request);
}