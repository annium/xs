using System;
using System.Threading.Tasks;
using NodaTime;
using Server.Domain.Models;

namespace Server.Db.Repositories;

internal interface IUserSessionRepository
{
    Task<UserSession> CreateAsync(UserSession userSession);

    Task<UserSession?> TryFindByTokenAsync(Guid token);

    Task ProlongateAsync(Guid token, Instant expires);

    Task DeleteByTokenAsync(Guid token);

    Task DeleteExpiredAsync(Instant now);
}