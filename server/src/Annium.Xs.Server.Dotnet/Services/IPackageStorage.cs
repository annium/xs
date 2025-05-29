using System.IO;
using System.Threading.Tasks;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Dotnet.Domain;

namespace Annium.Xs.Server.Dotnet.Services;

public interface IPackageStorage : IPackageStorage<Package, PackageDependency>
{
    Task<Stream> GetPackageAsync(string name, string version);
    Task<Stream> GetNuspecAsync(string name, string version);
}
