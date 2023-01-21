using System;
using System.Collections.Generic;

namespace Xs.Registry.Db.Shared.Models;

public interface IPackage<TPackageDependency> : IPackageInfo where TPackageDependency : IPackageDependency
{
    Guid Id { get; }

    Guid MetaPackageId { get; }

    int Downloads { get; }

    IEnumerable<TPackageDependency> Dependencies { get; }
}