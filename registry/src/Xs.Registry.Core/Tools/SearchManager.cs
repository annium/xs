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

        public SearchManager(
            IPackageRepository<TPackage> packageRepository
        )
        {
            this.packageRepository = packageRepository;
        }

        public async Task<IPackage[]> FindPackagesAsync(string query)
        {
            var packages = (await packageRepository.FindAllByQueryAsync(query)).OrderByDescending(e => e.Version);

            var result = new List<IPackage>();

            foreach (var package in packages)
                if (!result.Any(p => p.Name == package.Name))
                    result.Add(new PackagePreview(package));

            return result.ToArray();
        }

        public async Task<IPackage> FindLatestPackageAsync(string name)
        {
            return (await packageRepository.FindAllByNameAsync(name))
                .OrderByDescending(e => e.Version).FirstOrDefault();
        }

        public async Task<IPackage> FindPackageAsync(string name, string version)
        {
            var package = await packageRepository.FindByNameVersionAsync(name, version);

            return package;
        }
    }
}