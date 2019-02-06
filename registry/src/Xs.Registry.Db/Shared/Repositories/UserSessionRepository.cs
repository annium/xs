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

        public async Task<UserSession> CreateAsync(UserSession userSession)
        {
            var entity = mapper.Map<Entities.UserSession>(userSession);

            context.Entry(entity).State = EntityState.Added;

            await context.SaveChangesAsync();

            context.Entry(entity).State = EntityState.Detached;

            return mapper.Map<UserSession>(entity);
        }

        public async Task<UserSession> FindByTokenAsync(Guid token)
        {
            var entity = await context.UserSessions.FirstOrDefaultAsync(s => s.Token == token);

            return mapper.Map<UserSession>(entity);
        }

        public Task ProlongateAsync(Guid token, Instant expires)
        {
            return context.UserSessions
                .Where(s => s.Token == token)
                .UpdateAsync(s => new Entities.UserSession() { Expires = expires });
        }

        public Task DeleteByTokenAsync(Guid token)
        {
            return context.UserSessions.Where(s => s.Token == token).DeleteAsync();
        }

        public Task DeleteExpiredAsync(Instant now)
        {
            return context.UserSessions.Where(s => s.Expires < now).DeleteAsync();
        }
    }
}