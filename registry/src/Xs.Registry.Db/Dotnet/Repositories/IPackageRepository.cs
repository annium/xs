using System.Threading.Tasks;

namespace Xs.Registry.Db.Dotnet
{
    public interface IPackageRepository
    {
        Task CreateAsync(Package package);

        Task<Package> FindByNameVersionAsync(string name, string version);
    }
}