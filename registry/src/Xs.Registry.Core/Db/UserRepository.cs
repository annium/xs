using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Xs.Registry.Core.Models;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Core.Db
{
    internal class UserRepository : IUserRepository
    {
        private readonly CoreDbContext context;

        private readonly IMapper mapper;

        public UserRepository(
            CoreDbContext context,
            IMapper mapper
        )
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task CreateAsync(User user)
        {
            var entity = mapper.Map<Models.User>(user);
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
            var entity = mapper.Map<Models.User>(user);
            await context.Users
                .Where(u => u.Id == entity.Id)
                .UpdateAsync(u => new Models.User { Name = entity.Name, PasswordHash = entity.PasswordHash });
        }

        public async Task UpdateApiTokenAsync(Guid userId, Guid apiToken)
        {
            await context.Users
                .Where(u => u.Id == userId)
                .UpdateAsync(u => new Models.User { ApiToken = apiToken });
        }

        public async Task DeleteByNameAsync(string name)
        {
            await context.Users.Where(u => u.Name == name).DeleteAsync();
            await context.SaveChangesAsync();
        }
    }
}