using System.IO;
using System.Threading.Tasks;

namespace Server.Abstractions.Services;

public interface IStorage
{
    Task<bool> ExistsAsync(string name);
    Task<Stream> GetAsync(string name);
    Task SaveAsync(string name, Stream stream);
    Task DeleteAsync(string name);
}