using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xs.Registry.Core.Models;

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

        public Task<User> FindByNameAsync(string name) =>
            FindByPredicateAsync(u => u.Name == name);

        public Task<User> FindByApiTokenAsync(Guid token) =>
            FindByPredicateAsync(u => u.ApiToken == token);

        public Task<User> FindBySessionTokenAsync(Guid token) =>
            FindByPredicateAsync(u => u.Sessions.Any(s => s.Token == token));

        public async Task SaveAsync(User user)
        {
            var m = (Models.User) user;

            await collection.FindOneAndUpdateAsync(
                Builders<Models.User>.Filter.And(
                    Builders<Models.User>.Filter.Eq(e => e.Name, m.Name)
                ),
                Builders<Models.User>.Update
                .Set(u => u.Name, m.Name)
                .Set(u => u.PasswordHash, m.PasswordHash)
                .Set(u => u.ApiToken, m.ApiToken)
                .Set(u => u.Sessions, m.Sessions),
                new FindOneAndUpdateOptions<Models.User, Models.User>() { IsUpsert = true }
            );
        }

        public async Task DeleteByNameAsync(string name)
        {
            await collection.DeleteOneAsync(e => e.Name == name);
        }

        private async Task<User> FindByPredicateAsync(Expression<Func<Models.User, bool>> predicate)
        {
            var result = await collection.Find(predicate).FirstOrDefaultAsync();

            return (User) result;
        }
    }
}