using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Abstractions.Services;

public interface IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
    where TPackageRequest : class, IPackageRequest
{
    TPackage Parse(MetaPackage metaPackage, TPackageRequest request);
}
