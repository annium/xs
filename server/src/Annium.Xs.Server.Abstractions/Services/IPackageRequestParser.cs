using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Abstractions.Services;

public interface IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>
    where TPackage : IPackage<TPackageDependency>
    where TPackageDependency : IPackageDependency
    where TPackageRequest : IPackageRequest
{
    TPackage Parse(MetaPackage metaPackage, TPackageRequest request);
}
