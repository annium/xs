using System;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using LinqToDB;

namespace Xs.Registry.Db.Shared
{
    internal class UserRepository : IUserRepository
    {
        private readonly ISharedContext context;

        private readonly IMapper mapper;

        public UserRepository(
            ISharedContext context,
            IMapper mapper
        )
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<User> CreateAsync(User user)
        {
            var entity = mapper.Map<Entities.User>(user);
            entity.Id = Guid.NewGuid();

            using(var db = context.GetDataConnection())
            {
                await db.InsertAsync(entity);
            }

            return mapper.Map<User>(entity);
        }

        public async Task<User> GetById(Guid id)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            return mapper.Map<User>(user);
        }

        public async Task<User> FindByNameAsync(string name)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Name == name);

            return mapper.Map<User>(user);
        }

        public async Task<User> FindByApiTokenAsync(Guid token)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.ApiToken == token);

            return mapper.Map<User>(user);
        }

        public Task UpdateAsync(User user)
        {
            var entity = mapper.Map<Entities.User>(user);

            return context.Users
                .UpdateAsync(
                    u => u.Id == entity.Id,
                    u => new Entities.User
                    {
                        Name = entity.Name,
                            PasswordHash = entity.PasswordHash,
                            ApiToken = entity.ApiToken,
                    }
                );
        }

        public Task UpdateApiTokenAsync(Guid userId, Guid apiToken)
        {
            return context.Users
                .UpdateAsync(
                    u => u.Id == userId,
                    u => new Entities.User { ApiToken = apiToken, }
                );
        }

        public Task DeleteByIdAsync(Guid id)
        {
            return context.Users.DeleteAsync(u => u.Id == id);
        }
    }
}