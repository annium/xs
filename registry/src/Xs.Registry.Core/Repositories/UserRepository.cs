using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<Models.User> collection;

        public UserRepository(
            IMongoCollection<Models.User> collection
        )
        {
            this.collection = collection;
        }

        public async Task<User> FindByNameAsync(string name)
        {
            var m = await collection
                .Find(e => e.Name == name)
                .FirstOrDefaultAsync();

            return (User) m;
        }

        public async Task<User> FindByTokenAsync(string token)
        {
            var m = await collection
                .Find(e => e.Token == token)
                .FirstOrDefaultAsync();

            return (User) m;
        }

        public async Task SaveAsync(User user)
        {
            var m = (Models.User) user;

            await collection.FindOneAndUpdateAsync(
                Builders<Models.User>.Filter.And(
                    Builders<Models.User>.Filter.Eq(e => e.Name, m.Name)
                ),
                Builders<Models.User>.Update
                .Set(e => e.Name, m.Name)
                .Set(e => e.PasswordHash, m.PasswordHash)
                .Set(e => e.Token, m.Token),
                new FindOneAndUpdateOptions<Models.User, Models.User>() { IsUpsert = true }
            );
        }

        public async Task DeleteByNameAsync(string name)
        {
            await collection.DeleteOneAsync(e => e.Name == name);
        }
    }
}