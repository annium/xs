using System;

namespace Xs.Registry.Db.Shared.Entities;

public interface IPackageDependency
{
    Guid PackageId { get; set; }

    string Name { get; }

    string Version { get; }
}