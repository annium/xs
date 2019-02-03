using System.IO;
using System.Threading.Tasks;
using Xs.Registry.Core.Storage;
using Xs.Registry.Node.Models;

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

        public Task<bool> ExistsAsync(PackageName name, string version)
        {
            return storage.ExistsAsync(GetPackagePath(name, version));
        }

        public Task<Stream> GetAsync(PackageName name, string version)
        {
            return storage.GetAsync(GetPackagePath(name, version));
        }

        public Task SaveAsync(PackageName name, string version, Stream packageStream)
        {
            if (packageStream.CanSeek)
                packageStream.Position = 0;
            return storage.SaveAsync(GetPackagePath(name, version), packageStream);
        }

        public Task DeleteAsync(PackageName name, string version) =>
            storage.DeleteAsync(GetPackagePath(name, version));

        private string GetPackagePath(PackageName name, string version) =>
            Path.Combine(name.ToFileName(), $"{version}.tgz");
    }
}