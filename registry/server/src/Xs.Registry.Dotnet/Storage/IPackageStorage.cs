using System.IO;
using System.Threading.Tasks;

namespace Xs.Registry.Dotnet.Storage
{
    public interface IPackageStorage : Abstract.Packages.IPackageStorage
    {
        Task<Stream> GetPackageAsync(string name, string version);

        Task<Stream> GetNuspecAsync(string name, string version);
    }
}