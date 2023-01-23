using System;
using System.Collections.Generic;

namespace Server.Shared.Domain.Interfaces;

public interface IPackage<TPackageDependency> : IPackageInfo
    where TPackageDependency : IPackageDependency
{
    Guid Id { get; }
    Guid MetaPackageId { get; }
    int Downloads { get; }
    IReadOnlyCollection<TPackageDependency> Dependencies { get; }
}