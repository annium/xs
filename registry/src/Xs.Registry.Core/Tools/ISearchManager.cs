using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    public interface ISearchManager
    {
        Task<IPackage[]> FindPackagesAsync(string query);

        Task<IPackage> FindLatestPackageAsync(string name);

        Task<IPackage> FindPackageAsync(string name, string version);
    }
}