using System;
using System.Threading.Tasks;

namespace Xs.Registry.Db.Node
{
    public interface IPackageRepository
    {
        Task<Package> CreateAsync(Package package);

        Task<Package[]> FindAllByNameAsync(string name);

        Task<string[]> FindAllVersionsByNameAsync(string name);

        Task<Package> FindByNameVersionAsync(string name, string version);
        
        Task<int> CountAllDownloadsAsync(string name);

        Task IncrementDownloadsAsync(Guid id);

        Task DeleteByNameVersionAsync(string name, string version);
    }
}