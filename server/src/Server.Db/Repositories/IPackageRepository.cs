using System;
using System.Threading.Tasks;
using Server.Domain.Interfaces;

namespace Server.Db.Repositories;

internal interface IPackageRepository<TPackage, TPackageDependency> where TPackage : class, IPackage<TPackageDependency> where TPackageDependency : class, IPackageDependency
{
    Task<TPackage> CreateAsync(TPackage package);

    Task<TPackage[]> FindAllByNameAsync(string name);

    Task<string[]> FindAllVersionsByNameAsync(string name);

    Task<TPackage> FindByNameVersionAsync(string name, string version);

    Task<int> CountAllDownloadsAsync(string name);

    Task IncrementDownloadsAsync(Guid id);

    Task DeleteByNameVersionAsync(string name, string version);
}