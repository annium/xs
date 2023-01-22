using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Domain.Interfaces;

namespace Server.Db.Repositories;

internal interface IPackageRepository<TPackage, TPackageDependency>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
{
    Task CreateAsync(TPackage package);
    Task<IReadOnlyCollection<TPackage>> FindAllByNameAsync(string name);
    Task<TPackage?> TryFindByNameVersionAsync(string name, string version);
    Task<int> CountAllDownloadsAsync(string name);
    Task IncrementDownloadsAsync(Guid id);
    Task DeleteByNameVersionAsync(string name, string version);
}