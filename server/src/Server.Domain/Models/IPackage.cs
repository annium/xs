using System;
using System.Collections.Generic;

namespace Server.Domain.Models;

public interface IPackage<TPackageDependency> : IPackageInfo where TPackageDependency : IPackageDependency
{
    Guid Id { get; }

    Guid MetaPackageId { get; }

    int Downloads { get; }

    IEnumerable<TPackageDependency> Dependencies { get; }
}