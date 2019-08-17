using System;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using LinqToDB;
using NodaTime;

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

            await context.UserSessions
                .InsertAsync(() => new Entities.UserSession
                {
                    UserId = entity.UserId,
                        Token = entity.Token,
                        Expires = entity.Expires,
                });

            return mapper.Map<UserSession>(entity);
        }

        public async Task<UserSession> FindByTokenAsync(Guid token)
        {
            var entity = await context.UserSessions.FirstOrDefaultAsync(u => u.Token == token);

            return mapper.Map<UserSession>(entity);
        }

        public Task ProlongateAsync(Guid token, Instant expires)
        {
            var expiresDate = mapper.Map<DateTime>(expires);

            return context.UserSessions
                .UpdateAsync(
                    s => s.Token == token,
                    s => new Entities.UserSession { Expires = expiresDate }
                );
        }

        public Task DeleteByTokenAsync(Guid token)
        {
            return context.UserSessions.DeleteAsync(s => s.Token == token);
        }

        public Task DeleteExpiredAsync(Instant now)
        {
            var nowDate = mapper.Map<DateTime>(now);

            return context.UserSessions.DeleteAsync(s => s.Expires < nowDate);
        }
    }
}