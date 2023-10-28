using System.IO;
using System.Threading.Tasks;
using Server.Abstractions.Services;
using Server.Dotnet.Domain;

namespace Server.Dotnet.Services;

public interface IPackageStorage : IPackageStorage<Package, PackageDependency>
{
    Task<Stream> GetPackageAsync(string name, string version);
    Task<Stream> GetNuspecAsync(string name, string version);
}
