using System.IO;
using System.Threading.Tasks;
using Server.Abstractions.Services;

namespace Server.Node.Storage;

internal class PackageStorage : IPackageStorage
{
    private readonly IStorage _storage;

    private readonly Configuration _configuration;

    public PackageStorage(
        IStorageFactory storageFactory,
        Configuration configuration
    )
    {
        _storage = storageFactory.Create(configuration.PackagesFolder);
        _configuration = configuration;
    }

    public Task<bool> ExistsAsync(string name, string version)
    {
        return _storage.ExistsAsync(GetPackagePath(name, version));
    }

    public Task SaveAsync(string name, string version, Stream packageStream)
    {
        if (packageStream.CanSeek)
            packageStream.Position = 0;
        return _storage.SaveAsync(GetPackagePath(name, version), packageStream);
    }

    public Task DeleteAsync(string name, string version) =>
        _storage.DeleteAsync(GetPackagePath(name, version));

    public Task<Stream> GetAsync(string name, string version)
    {
        return _storage.GetAsync(GetPackagePath(name, version));
    }

    private string GetPackagePath(string name, string version) =>
        Path.Combine(name.ToLowerInvariant().Replace("@", string.Empty).Replace("/", "-"), $"{version}.tgz");
}