using System.IO;
using System.Threading.Tasks;
using Server.Domain.Interfaces;

namespace Server.Abstractions.Services;

public interface IPackageStorage<TPackage, TPackageDependency>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
{
    Task<bool> ExistsAsync(string name, string version);
    Task SaveAsync(string name, string version, Stream stream);
    Task DeleteAsync(string name, string version);
}