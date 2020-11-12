using System.IO;
using System.Threading.Tasks;
using NuGet.Packaging;
using Xs.Registry.Abstract.Storage;

namespace Xs.Registry.Dotnet.Storage
{
    internal class PackageStorage : IPackageStorage
    {
        private readonly IStorage _storage;

        public PackageStorage(
            IStorageFactory storageFactory,
            Configuration configuration
        )
        {
            _storage = storageFactory.Create(configuration.PackagesFolder);
        }

        public async Task<bool> ExistsAsync(string name, string version)
        {
            return await _storage.ExistsAsync(GetPackagePath(name, version)) &&
                await _storage.ExistsAsync(GetNuspecPath(name, version));
        }

        public async Task SaveAsync(string name, string version, Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;
            await _storage.SaveAsync(GetPackagePath(name, version), stream);

            using(var packageReader = new PackageArchiveReader(stream, leaveStreamOpen : true))
            {
                var nuspecStream = packageReader.GetNuspec();
                if (nuspecStream.CanSeek)
                    nuspecStream.Position = 0;
                await _storage.SaveAsync(GetNuspecPath(name, version), nuspecStream);
            }
        }

        public async Task DeleteAsync(string name, string version)
        {
            await _storage.DeleteAsync(GetPackagePath(name, version));
            await _storage.DeleteAsync(GetNuspecPath(name, version));
        }

        public Task<Stream> GetPackageAsync(string name, string version)
        {
            return _storage.GetAsync(GetPackagePath(name, version));
        }

        public Task<Stream> GetNuspecAsync(string name, string version)
        {
            return _storage.GetAsync(GetNuspecPath(name, version));
        }

        private string GetPackagePath(string name, string version) =>
            Path.Combine(name.ToLowerInvariant(), $"{version}.nupkg");

        private string GetNuspecPath(string name, string version) =>
            Path.Combine(name.ToLowerInvariant(), $"{version}.nuspec");
    }
}