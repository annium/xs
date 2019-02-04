using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

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

        public async Task CreateAsync(User user)
        {
            var entity = mapper.Map<Entities.User>(user);
            context.Users.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task<User> GetById(Guid id)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();

            return mapper.Map<User>(user);
        }

        public async Task<User> FindByNameAsync(string name)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.Name == name)
                .FirstOrDefaultAsync();

            return mapper.Map<User>(user);
        }

        public async Task<User> FindByApiTokenAsync(Guid token)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.ApiToken == token)
                .FirstOrDefaultAsync();

            return mapper.Map<User>(user);
        }

        public async Task UpdateAsync(User user)
        {
            var entity = mapper.Map<Entities.User>(user);
            await context.Users
                .Where(u => u.Id == entity.Id)
                .UpdateAsync(u => new Entities.User
                {
                    Name = entity.Name,
                        PasswordHash = entity.PasswordHash,
                        ApiToken = entity.ApiToken,
                });
        }

        public async Task UpdateApiTokenAsync(Guid userId, Guid apiToken)
        {
            await context.Users
                .Where(u => u.Id == userId)
                .UpdateAsync(u => new Entities.User { ApiToken = apiToken });
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            await context.Users.Where(u => u.Id == id).DeleteAsync();
            await context.SaveChangesAsync();
        }
    }
}