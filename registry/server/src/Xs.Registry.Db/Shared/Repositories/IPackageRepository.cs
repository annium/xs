using System;
using System.Threading.Tasks;

namespace Xs.Registry.Db.Shared
{
    public interface IPackageRepository<TPackage> where TPackage : class, IPackage
    {
        Task<TPackage> CreateAsync(TPackage package);

        Task<TPackage[]> FindAllByNameAsync(string name);

        Task<string[]> FindAllVersionsByNameAsync(string name);

        Task<TPackage> FindByNameVersionAsync(string name, string version);

        Task<int> CountAllDownloadsAsync(string name);

        Task IncrementDownloadsAsync(Guid id);

        Task DeleteByNameVersionAsync(string name, string version);
    }
}