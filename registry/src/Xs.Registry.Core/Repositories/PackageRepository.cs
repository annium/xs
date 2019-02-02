using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public class PackageRepository<TPackage, TPackageModel> : IPackageRepository<TPackage>
        where TPackage : IPackage
    where TPackageModel : IPackage
    {
        private static TPackageModel ToModel(TPackage package) => (TPackageModel) (object) package;

        private static TPackage ToPackage(TPackageModel package) => (TPackage) (object) package;

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

        public Task<TPackage[]> FindAllByMetaPackageIdAsync(string metaPackageId) =>
            FindAllByPredicateAsync(e => e.MetaPackageId == metaPackageId);

        public Task<TPackage[]> FindAllByNameAsync(string name)
        {
            name = name.ToLowerInvariant();

            return FindAllByPredicateAsync(e => e.Name.ToLowerInvariant() == name);
        }

        public Task<TPackage[]> FindAllByQueryAsync(string query)
        {
            query = query.ToLowerInvariant();

            return FindAllByPredicateAsync(e => e.Name.ToLowerInvariant().Contains(query));
        }

        public Task<TPackage> FindLatestByNameAsync(string name)
        {
            name = name.ToLowerInvariant();

            return FindByPredicateAsync(e => e.Name.ToLowerInvariant() == name);
        }

        public Task<TPackage> FindByNameVersionAsync(string name, string version)
        {
            name = name.ToLowerInvariant();
            version = version.ToLowerInvariant();

            return FindByPredicateAsync(e =>
                e.Name.ToLowerInvariant() == name &&
                e.Version.ToLowerInvariant() == version
            );
        }

        public async Task SaveAsync(TPackage package)
        {
            var model = ToModel(package);

            await collection.InsertOneAsync(model);
        }

        public async Task DeleteByNameVersionAsync(string name, string version)
        {
            name = name.ToLowerInvariant();
            version = version.ToLowerInvariant();

            await collection
                .DeleteOneAsync(e => e.Name.ToLowerInvariant() == name && e.Version.ToLowerInvariant() == version);
        }

        private async Task<TPackage[]> FindAllByPredicateAsync(Expression<Func<TPackageModel, bool>> predicate)
        {
            var result = await collection
                .Find(predicate)
                .Sort(Builders<TPackageModel>.Sort.Descending(e => e.Version))
                .ToListAsync();

            return result.Select(ToPackage).ToArray();
        }

        private async Task<TPackage> FindByPredicateAsync(Expression<Func<TPackageModel, bool>> predicate)
        {
            var result = await collection
                .Find(predicate)
                .Sort(Builders<TPackageModel>.Sort.Descending(e => e.Version))
                .FirstOrDefaultAsync();

            return ToPackage(result);
        }
    }
}