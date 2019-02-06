using System.Threading.Tasks;

namespace Xs.Registry.Db.Dotnet
{
    public interface IPackageRepository
    {
        Task<Package> CreateAsync(Package package);

        Task<Package[]> FindAllByNameAsync(string name);

        Task<Package> FindByNameVersionAsync(string name, string version);
        
        Task<int> CountAllDownloadsAsync(string name);

        Task DeleteByNameVersionAsync(string name, string version);
    }
}