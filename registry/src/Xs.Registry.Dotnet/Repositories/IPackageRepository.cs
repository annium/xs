using System.Threading.Tasks;
using Xs.Registry.Dotnet.Models;

namespace Xs.Registry.Dotnet.Repositories
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