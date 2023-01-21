using System;
using Server.Domain.Models;

namespace Server.Abstractions.Packages;

public interface IPayloadParser<TPayload, TPackage, TPackageDependency> where TPayload : IPayload where TPackage : IPackage<TPackageDependency> where TPackageDependency : IPackageDependency
{
    TPackage Parse(Guid metaPackageId, TPayload payload);
}