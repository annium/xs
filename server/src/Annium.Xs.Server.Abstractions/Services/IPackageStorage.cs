using System.IO;
using System.Threading.Tasks;
using Annium.Xs.Server.Shared.Domain.Interfaces;

namespace Annium.Xs.Server.Abstractions.Services;

// ReSharper disable once UnusedTypeParameter
public interface IPackageStorage<TPackage, TPackageDependency>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
{
    Task<bool> ExistsAsync(string name, string version);
    Task SaveAsync(string name, string version, Stream stream);
    Task DeleteAsync(string name, string version);
    Task<Stream> GetAsync(string name, string version);
}
