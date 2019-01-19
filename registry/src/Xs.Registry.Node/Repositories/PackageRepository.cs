using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Registry.Node.Models;

namespace Xs.Registry.Node.Repositories
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

        public async Task<Package[]> FindAllByNameAsync(string name)
        {
            var models = await collection
                .Find(e => e.Name == name)
                .ToListAsync();

            return models.Select(e => (Package) e).ToArray();
        }

        public async Task<Package> FindByNameVersionAsync(string name, string version)
        {
            var model = await collection
                .Find(e => e.Name == name && e.Version == version)
                .FirstOrDefaultAsync();

            return (Package) model;
        }

        public async Task SaveAsync(Package package)
        {
            var model = (Models.Package) package;

            await collection.InsertOneAsync(model);
        }

        public Task DeleteAllByNameAsync(string name)
        {
            return collection.DeleteManyAsync(e => e.Name == name);
        }

        public Task DeleteByNameVersionAsync(string name, string version)
        {
            return collection.DeleteOneAsync(e => e.Name == name && e.Version == version);
        }
    }
}