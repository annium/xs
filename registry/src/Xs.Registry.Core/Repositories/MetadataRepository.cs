using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Core.Models;
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

        public async Task<Metadata> FindByProjectTypePackageNameAsync(ProjectType projectType, string packageName)
        {
            var projectTypeString = projectType.ToString();
            packageName = packageName.ToLowerInvariant();

            var m = await collection
                .Find(
                    e => e.ProjectType == projectTypeString &&
                    e.PackageName.ToLowerInvariant() == packageName
                )
                .FirstOrDefaultAsync();

            return (Metadata) m;
        }

        public async Task SaveAsync(Metadata metadata)
        {
            var m = (Models.Metadata) metadata;

            await collection.FindOneAndUpdateAsync(
                Builders<Models.Metadata>.Filter.And(
                    Builders<Models.Metadata>.Filter.Eq(e => e.ProjectType, m.ProjectType),
                    Builders<Models.Metadata>.Filter.Eq(e => e.PackageName, m.PackageName)
                ),
                Builders<Models.Metadata>.Update
                .Set(e => e.UserId, m.UserId)
                .Set(e => e.ProjectType, m.ProjectType)
                .Set(e => e.PackageName, m.PackageName)
                .Set(e => e.Permissions, m.Permissions),
                new FindOneAndUpdateOptions<Models.Metadata, Models.Metadata>() { IsUpsert = true }
            );
        }

        public async Task DeleteByProjectTypePackageNameAsync(ProjectType projectType, string packageName)
        {
            var projectTypeString = projectType.ToString();
            packageName = packageName.ToLowerInvariant();

            await collection.DeleteOneAsync(
                e => e.ProjectType == projectTypeString &&
                e.PackageName.ToLowerInvariant() == packageName
            );
        }
    }
}