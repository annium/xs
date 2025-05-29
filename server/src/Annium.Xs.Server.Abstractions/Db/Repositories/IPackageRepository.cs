using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Xs.Server.Shared.Domain.Interfaces;

namespace Annium.Xs.Server.Abstractions.Db.Repositories;

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
