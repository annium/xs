using Server.Abstractions.Domain;
using Server.Domain.Interfaces;
using Server.Domain.Models;

namespace Server.Abstractions.Services;

public interface IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>
    where TPackage : IPackage<TPackageDependency>
    where TPackageDependency : IPackageDependency
    where TPackageRequest : IPackageRequest
{
    TPackage Parse(MetaPackage metaPackage, TPackageRequest request);
}