using System.IO;
using System.Threading.Tasks;
using Server.Abstractions.Services;
using Server.Node.Models;

namespace Server.Node.Storage;

public interface IPackageStorage : IPackageStorage<Package, PackageDependency>
{
    Task<Stream> GetAsync(string name, string version);
}