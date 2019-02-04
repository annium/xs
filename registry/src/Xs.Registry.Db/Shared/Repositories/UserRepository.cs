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

        public async Task<User> FindByNameAsync(string name)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.Name == name)
                .FirstOrDefaultAsync();

            return mapper.Map<User>(user);
        }
    }
}