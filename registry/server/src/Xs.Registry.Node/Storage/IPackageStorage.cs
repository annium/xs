using System.IO;
using System.Threading.Tasks;

namespace Xs.Registry.Node.Storage
{
    public interface IPackageStorage
    {
        Task<bool> ExistsAsync(string name, string version);

        Task<Stream> GetAsync(string name, string version);

        Task SaveAsync(string name, string version, Stream packageStream);

        Task DeleteAsync(string name, string version);
    }
}