using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    internal class MetadataRepository : IMetadataRepository
    {
        private readonly IMongoCollection<Models.Metadata> collection;

        public MetadataRepository(
            IMongoCollection<Models.Metadata> collection
        )
        {
            this.collection = collection;
        }

        public async Task<Metadata[]> GetByIdsAsync(string[] ids)
        {
            var result = await collection
                .Find(e => ids.Any(id => e.Id == id))
                .ToListAsync();

            return result.Select(e => (Metadata) e).ToArray();
        }

        public async Task<Metadata> GetByIdAsync(string id)
        {
            var m = await collection
                .Find(e => e.Id == id)
                .FirstOrDefaultAsync();

            return (Metadata) m;
        }

        public async Task<Metadata[]> FindAllByOwnerIdAsync(string ownerId)
        {
            var result = await collection
                .Find(e => e.OwnerId == ownerId)
                .ToListAsync();

            return result.Select(e => (Metadata) e).ToArray();
        }

        public async Task SaveAsync(Metadata metadata)
        {
            var m = (Models.Metadata) metadata;

            await collection.FindOneAndUpdateAsync(
                Builders<Models.Metadata>.Filter.And(
                    Builders<Models.Metadata>.Filter.Eq(e => e.Id, m.Id)
                ),
                Builders<Models.Metadata>.Update
                .Set(e => e.OwnerId, m.OwnerId)
                .Set(e => e.Permissions, m.Permissions),
                new FindOneAndUpdateOptions<Models.Metadata, Models.Metadata>() { IsUpsert = true }
            );
        }

        public async Task DeleteByIdAsync(string id)
        {
            await collection.DeleteOneAsync(e => e.Id == id);
        }
    }
}