using System;
using System.Threading.Tasks;
using NodaTime;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Db
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