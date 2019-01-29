using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Tools
{
    public interface ISearchManager
    {
        Task<PackagePreview[]> FindPackagesAsync(string query);

        Task<PackagePreview> FindLatestPackageAsync(string name);

        Task<PackagePreview> FindPackageAsync(string name, string version);
    }
}