using System.IO;
using System.Threading.Tasks;

namespace Xs.Registry.Dotnet.Storage
{
    public interface IPackageStorage
    {
        Task<bool> ExistsAsync(string name, string version);

        Task<Stream> GetPackageAsync(string name, string version);

        Task<Stream> GetNuspecAsync(string name, string version);

        Task SaveAsync(string name, string version, Stream packageStream, Stream nuspecStream);

        Task DeleteAsync(string name, string version);
    }
}