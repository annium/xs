using System.Threading.Tasks;
using Xs.Registry.Node.Models;

namespace Xs.Registry.Node.Repositories
{
    public interface IPackageRepository
    {
        Task<Package[]> FindAllByNameAsync(string name);

        Task<Package> FindByNameVersionAsync(string name, string version);

        Task SaveAsync(Package package);

        Task DeleteAllByNameAsync(string name);

        Task DeleteByNameVersionAsync(string name, string version);
    }
}