using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Db.Shared
{
    internal class UserSessionRepository : IUserSessionRepository
    {
        private readonly ISharedContext context;

        private readonly IMapper mapper;

        public UserSessionRepository(
            ISharedContext context,
            IMapper mapper
        )
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task CreateAsync(UserSession userSession)
        {
            var entity = mapper.Map<Entities.UserSession>(userSession);
            context.UserSessions.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task<UserSession> FindByTokenAsync(Guid token)
        {
            var entity = await context.UserSessions.FirstOrDefaultAsync(s => s.Token == token);

            return mapper.Map<UserSession>(entity);
        }

        public async Task ProlongateAsync(Guid token, Instant expires)
        {
            await context.UserSessions
                .Where(s => s.Token == token)
                .UpdateAsync(s => new Entities.UserSession() { Expires = expires });
        }

        public async Task DeleteByTokenAsync(Guid token)
        {
            await context.UserSessions.Where(s => s.Token == token).DeleteAsync();
        }

        public async Task DeleteExpiredAsync(Instant now)
        {
            await context.UserSessions.Where(s => s.Expires < now).DeleteAsync();
        }
    }
}