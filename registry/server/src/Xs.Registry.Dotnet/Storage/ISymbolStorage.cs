using System.IO;
using System.Threading.Tasks;

namespace Xs.Registry.Dotnet.Storage;

public interface ISymbolStorage
{
    Task<bool> ExistsAsync(string name, string version);

    Task<Stream> GetFileAsync(string name, string version, string file);

    Task SaveAsync(string name, string version, Stream symbolStream);

    Task DeleteAsync(string name, string version);
}