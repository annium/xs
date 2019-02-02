using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Db
{
    internal class MetaPackageRepository : IMetaPackageRepository
    {
        private readonly IMongoCollection<Models.MetaPackage> collection;

        public MetaPackageRepository(
            IMongoCollection<Models.MetaPackage> collection
        )
        {
            this.collection = collection;
        }

        public async Task<MetaPackage[]> GetByIdsAsync(string[] ids)
        {
            var result = await collection
                .Find(e => ids.Any(id => e.Id == id))
                .ToListAsync();

            return result.Select(e => (MetaPackage) e).ToArray();
        }

        public async Task<MetaPackage> GetByIdAsync(string id)
        {
            var m = await collection
                .Find(e => e.Id == id)
                .FirstOrDefaultAsync();

            return (MetaPackage) m;
        }

        public async Task<MetaPackage[]> FindAllByOwnerIdAsync(string ownerId)
        {
            var result = await collection
                .Find(e => e.OwnerId == ownerId)
                .ToListAsync();

            return result.Select(e => (MetaPackage) e).ToArray();
        }

        public async Task SaveAsync(MetaPackage metaPackage)
        {
            var m = (Models.MetaPackage) metaPackage;

            await collection.FindOneAndUpdateAsync(
                Builders<Models.MetaPackage>.Filter.And(
                    Builders<Models.MetaPackage>.Filter.Eq(e => e.Id, m.Id)
                ),
                Builders<Models.MetaPackage>.Update
                .Set(e => e.OwnerId, m.OwnerId)
                .Set(e => e.Permissions, m.Permissions),
                new FindOneAndUpdateOptions<Models.MetaPackage, Models.MetaPackage>() { IsUpsert = true }
            );
        }

        public async Task DeleteByIdAsync(string id)
        {
            await collection.DeleteOneAsync(e => e.Id == id);
        }
    }
}