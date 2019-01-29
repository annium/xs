using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public interface IPackageRepository<TPackage> where TPackage : IPackage
    {
        Task<TPackage[]> FindAllByQueryAsync(string query);

        Task<TPackage[]> FindAllByNameAsync(string name);

        Task<TPackage> FindLatestByNameAsync(string name);

        Task<TPackage> FindByNameVersionAsync(string name, string version);

        Task SaveAsync(TPackage package);

        Task DeleteAllByNameAsync(string name);

        Task DeleteByNameVersionAsync(string name, string version);
    }
}