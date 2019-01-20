using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Registry.Dotnet.Models;

namespace Xs.Registry.Dotnet.Repositories
{
    internal class PackageRepository : IPackageRepository
    {
        private readonly IMongoCollection<Models.Package> collection;

        private readonly Configuration configuration;

        private readonly Func<DateTime> getTime;

        public PackageRepository(
            IMongoCollection<Models.Package> collection,
            Configuration configuration,
            Func<DateTime> getTime
        )
        {
            this.collection = collection;
            this.configuration = configuration;
            this.getTime = getTime;
        }

        public async Task<Package[]> FindAllByQueryAsync(string query)
        {
            query = query.ToLowerInvariant();

            var models = await collection
                .Find(e => e.Name.ToLowerInvariant().Contains(query))
                .ToListAsync();

            return models.Select(e => (Package) e).ToArray();
        }

        public async Task<Package[]> FindAllByNameAsync(string name)
        {
            name = name.ToLowerInvariant();

            var models = await collection
                .Find(e => e.Name.ToLowerInvariant() == name)
                .ToListAsync();

            return models.Select(e => (Package) e).ToArray();
        }

        public async Task<Package> FindByNameVersionAsync(string name, string version)
        {
            name = name.ToLowerInvariant();
            version = version.ToLowerInvariant();

            var model = await collection
                .Find(e => e.Name.ToLowerInvariant() == name && e.Version.ToLowerInvariant() == version)
                .FirstOrDefaultAsync();

            return (Package) model;
        }

        public async Task SaveAsync(Package package)
        {
            var model = (Models.Package) package;

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