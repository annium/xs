using System;
using System.Threading.Tasks;
using NodaTime;
using Server.Domain.Models;

namespace Server.Db.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession> CreateAsync(UserSession userSession);

    Task<UserSession> FindByTokenAsync(Guid token);

    Task ProlongateAsync(Guid token, Instant expires);

    Task DeleteByTokenAsync(Guid token);

    Task DeleteExpiredAsync(Instant now);
}