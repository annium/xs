using System;
using Xs.Registry.Db.Shared.Models;

namespace Xs.Registry.Abstract.Packages;

public interface IPayloadParser<TPayload, TPackage, TPackageDependency> where TPayload : IPayload where TPackage : IPackage<TPackageDependency> where TPackageDependency : IPackageDependency
{
    TPackage Parse(Guid metaPackageId, TPayload payload);
}