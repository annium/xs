using System;
using Server.Domain.Interfaces;

namespace Server.Abstractions.Packages;

public interface IPayloadParser<TPackage, TPackageDependency, TPayload>
    where TPackage : IPackage<TPackageDependency>
    where TPackageDependency : IPackageDependency
    where TPayload : IPayload
{
    TPackage Parse(Guid metaPackageId, TPayload payload);
}