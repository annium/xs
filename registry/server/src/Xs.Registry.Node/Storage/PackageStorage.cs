using System.IO;
using System.Threading.Tasks;
using Xs.Registry.Abstract.Storage;

namespace Xs.Registry.Node.Storage
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

        public Task SaveAsync(string name, string version, Stream packageStream)
        {
            if (packageStream.CanSeek)
                packageStream.Position = 0;
            return storage.SaveAsync(GetPackagePath(name, version), packageStream);
        }

        public Task DeleteAsync(string name, string version) =>
            storage.DeleteAsync(GetPackagePath(name, version));

        public Task<Stream> GetAsync(string name, string version)
        {
            return storage.GetAsync(GetPackagePath(name, version));
        }

        private string GetPackagePath(string name, string version) =>
            Path.Combine(name.ToLowerInvariant().Replace("@", string.Empty).Replace("/", "-"), $"{version}.tgz");
    }
}