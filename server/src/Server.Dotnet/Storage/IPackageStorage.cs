using System.IO;
using System.Threading.Tasks;

namespace Server.Dotnet.Storage;

public interface IPackageStorage : Abstractions.Services.IPackageStorage
{
    Task<Stream> GetPackageAsync(string name, string version);

    Task<Stream> GetNuspecAsync(string name, string version);
}