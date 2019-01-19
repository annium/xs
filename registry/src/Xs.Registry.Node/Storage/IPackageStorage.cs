using System.IO;
using System.Threading.Tasks;
using Xs.Registry.Node.Models;

namespace Xs.Registry.Node.Storage
{
    public interface IPackageStorage
    {
        Task<bool> ExistsAsync(PackageName name, string version);

        Task<Stream> GetAsync(PackageName name, string version);

        Task SaveAsync(PackageName name, string version, Stream packageStream);

        Task DeleteAsync(PackageName name, string version);
    }
}