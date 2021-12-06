using System.IO;
using System.Threading.Tasks;

namespace Xs.Registry.Node.Storage;

public interface IPackageStorage : Abstract.Packages.IPackageStorage
{
    Task<Stream> GetAsync(string name, string version);
}