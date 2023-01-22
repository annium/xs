using System;
using Server.Abstractions.Domain;
using Server.Domain.Interfaces;

namespace Server.Abstractions.Services;

public interface IPayloadParser<TPackage, TPackageDependency, TPackagePayload>
    where TPackage : IPackage<TPackageDependency>
    where TPackageDependency : IPackageDependency
    where TPackagePayload : IPayload
{
    TPackage Parse(Guid metaPackageId, TPackagePayload payload);
}