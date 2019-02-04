using System;
using System.Threading.Tasks;
using NodaTime;

namespace Xs.Registry.Db.Shared
{
    public interface IUserSessionRepository
    {
        Task CreateAsync(UserSession userSession);

        Task<UserSession> FindByTokenAsync(Guid token);

        Task ProlongateAsync(Guid token, Instant expires);

        Task DeleteByTokenAsync(Guid token);

        Task DeleteExpiredAsync(Instant now);
    }
}