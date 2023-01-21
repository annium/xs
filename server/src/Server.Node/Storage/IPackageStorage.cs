using System.IO;
using System.Threading.Tasks;

namespace Server.Node.Storage;

public interface IPackageStorage : Abstractions.Packages.IPackageStorage
{
    Task<Stream> GetAsync(string name, string version);
}