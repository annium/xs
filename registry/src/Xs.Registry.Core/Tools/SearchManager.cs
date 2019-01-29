using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xs.Registry.Core.Models;
using Xs.Registry.Core.Repositories;

namespace Xs.Registry.Core.Tools
{
    public class SearchManager<TPackage> : ISearchManager
    where TPackage : IPackage
    {
        private readonly IPackageRepository<TPackage> packageRepository;

        private readonly IMetadataRepository metadataRepository;

        private readonly IUserRepository userRepository;

        public SearchManager(
            IPackageRepository<TPackage> packageRepository,
            IMetadataRepository metadataRepository,
            IUserRepository userRepository
        )
        {
            this.packageRepository = packageRepository;
            this.metadataRepository = metadataRepository;
            this.userRepository = userRepository;
        }

        public async Task<PackagePreview[]> FindPackagesAsync(string query)
        {
            //TODO: implement distinct on BE
            var allPackages = await packageRepository.FindAllByQueryAsync(query);
            if (allPackages.Length == 0)
                return Array.Empty<PackagePreview>();

            var packages = new List<IPackage>();
            foreach (var package in allPackages)
                if (!packages.Any(p => p.Name == package.Name))
                    packages.Add(package);

            var metadataIds = packages.Select(p => p.MetadataId).ToArray();
            var metadata = await metadataRepository.GetByIdsAsync(metadataIds);

            var userIds = metadata.Select(p => p.OwnerId).ToArray();
            var users = await userRepository.GetByIdsAsync(userIds);

            var result = new List<PackagePreview>();
            foreach (var package in packages)
            {
                var data = metadata.FirstOrDefault(m => m.Id == package.MetadataId);
                if (data == null)
                    throw new Exception($"Metadata {package.MetadataId} referenced by package {package.Id} is missing");
                var user = users.FirstOrDefault(u => u.Id == data.OwnerId);
                if (user == null)
                    throw new Exception($"User {data.OwnerId} referenced by metadata {data.Id} is missing");

                result.Add(new PackagePreview(package, data, user));
            }

            return result.ToArray();
        }

        public async Task<PackagePreview> FindLatestPackageAsync(string name)
        {
            var package = await packageRepository.FindLatestByNameAsync(name);
            if (package == null)
                return null;

            return await BuildPackagePreviewAsync(package);
        }

        public async Task<PackagePreview> FindPackageAsync(string name, string version)
        {
            var package = await packageRepository.FindByNameVersionAsync(name, version);
            if (package == null)
                return null;

            return await BuildPackagePreviewAsync(package);
        }

        private async Task<PackagePreview> BuildPackagePreviewAsync(IPackage package)
        {
            var metadata = await metadataRepository.GetByIdAsync(package.MetadataId);
            if (metadata == null)
                throw new Exception($"Metadata {package.MetadataId} referenced by package {package.Id} is missing");

            var user = await userRepository.GetByIdAsync(metadata.OwnerId);
            if (user == null)
                throw new Exception($"User {metadata.OwnerId} referenced by metadata {metadata.Id} is missing");

            return new PackagePreview(package, metadata, user);
        }
    }
}