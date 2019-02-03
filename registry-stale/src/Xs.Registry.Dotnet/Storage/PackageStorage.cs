using System.IO;
using System.Threading.Tasks;
using Xs.Registry.Core.Storage;

namespace Xs.Registry.Dotnet.Storage
{
    internal class PackageStorage : IPackageStorage
    {
        private readonly IStorage storage;

        private readonly Configuration configuration;

        public PackageStorage(
            IStorageFactory storageFactory,
            Configuration configuration
        )
        {
            this.storage = storageFactory.Create(configuration.PackagesFolder);
            this.configuration = configuration;
        }

        public Task<bool> ExistsAsync(string name, string version)
        {
            return storage.ExistsAsync(GetPackagePath(name, version));
        }

        public Task<Stream> GetPackageAsync(string name, string version)
        {
            return storage.GetAsync(GetPackagePath(name, version));
        }

        public Task<Stream> GetNuspecAsync(string name, string version)
        {
            return storage.GetAsync(GetNuspecPath(name, version));
        }

        public async Task SaveAsync(string name, string version, Stream packageStream, Stream nuspecStream)
        {
            if (packageStream.CanSeek)
                packageStream.Position = 0;
            await storage.SaveAsync(GetPackagePath(name, version), packageStream);

            if (nuspecStream.CanSeek)
                nuspecStream.Position = 0;
            await storage.SaveAsync(GetNuspecPath(name, version), nuspecStream);
        }

        public async Task DeleteAsync(string name, string version)
        {
            await storage.DeleteAsync(GetPackagePath(name, version));
            await storage.DeleteAsync(GetNuspecPath(name, version));
        }

        private string GetPackagePath(string name, string version) =>
            Path.Combine(name.ToLowerInvariant(), $"{name.ToLowerInvariant()}.{version}.nupkg");

        private string GetNuspecPath(string name, string version) =>
            Path.Combine(name.ToLowerInvariant(), $"{name.ToLowerInvariant()}.{version}.nuspec");
    }
}