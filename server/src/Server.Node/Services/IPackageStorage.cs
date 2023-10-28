using System.IO;
using System.Threading.Tasks;
using Server.Abstractions.Services;
using Server.Node.Domain;

namespace Server.Node.Services;

public interface IPackageStorage : IPackageStorage<Package, PackageDependency>
{
    Task<Stream> GetAsync(string name, string version);
}
