using System.IO;
using System.Threading.Tasks;

namespace Server.Abstractions.Services;

public interface IPackageStorage
{
    Task<bool> ExistsAsync(string name, string version);
    Task SaveAsync(string name, string version, Stream stream);
    Task DeleteAsync(string name, string version);
}