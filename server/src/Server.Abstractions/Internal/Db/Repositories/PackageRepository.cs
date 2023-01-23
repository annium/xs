using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Abstractions.Db.Repositories;
using Server.Domain.Interfaces;

namespace Server.Abstractions.Internal.Db.Repositories;

internal class PackageRepository<TPackage, TPackageDependency> : IPackageRepository<TPackage, TPackageDependency>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
{
    public Task CreateAsync(TPackage package)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<TPackage>> FindAllByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<TPackage?> TryFindByNameVersionAsync(string name, string version)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAllDownloadsAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task IncrementDownloadsAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task DeleteByNameVersionAsync(string name, string version)
    {
        throw new NotImplementedException();
    }
}