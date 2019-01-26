using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public class PackageRepository<TPackage, TPackageModel> : IPackageRepository<TPackage>
        where TPackage : IPackage
    where TPackageModel : IPackage
    {
        private readonly IMongoCollection<TPackageModel> collection;

        private readonly Configuration configuration;

        private readonly Func<DateTime> getTime;

        public PackageRepository(
            IMongoCollection<TPackageModel> collection,
            Configuration configuration,
            Func<DateTime> getTime
        )
        {
            this.collection = collection;
            this.configuration = configuration;
            this.getTime = getTime;
        }

        public async Task<TPackage[]> FindAllByQueryAsync(string query)
        {
            query = query.ToLowerInvariant();

            var models = await collection
                .Find(e => e.Name.ToLowerInvariant().Contains(query))
                .ToListAsync();

            return models.Select(e => (TPackage) (object) e).ToArray();
        }

        public async Task<TPackage[]> FindAllByNameAsync(string name)
        {
            name = name.ToLowerInvariant();

            var models = await collection
                .Find(e => e.Name.ToLowerInvariant() == name)
                .ToListAsync();

            return models.Select(e => (TPackage) (object) e).ToArray();
        }

        public async Task<TPackage> FindByNameVersionAsync(string name, string version)
        {
            name = name.ToLowerInvariant();
            version = version.ToLowerInvariant();

            var model = await collection
                .Find(e => e.Name.ToLowerInvariant() == name && e.Version.ToLowerInvariant() == version)
                .FirstOrDefaultAsync();

            return (TPackage) (object) model;
        }

        public async Task SaveAsync(TPackage package)
        {
            var model = (TPackageModel) (object) package;

            await collection.InsertOneAsync(model);
        }

        public async Task DeleteAllByNameAsync(string name)
        {
            name = name.ToLowerInvariant();

            await collection.DeleteManyAsync(e => e.Name.ToLowerInvariant() == name);
        }

        public async Task DeleteByNameVersionAsync(string name, string version)
        {
            name = name.ToLowerInvariant();
            version = version.ToLowerInvariant();

            await collection
                .DeleteOneAsync(e => e.Name.ToLowerInvariant() == name && e.Version.ToLowerInvariant() == version);
        }
    }
}