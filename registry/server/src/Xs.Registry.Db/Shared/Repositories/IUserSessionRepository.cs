using System;
using System.Threading.Tasks;
using NodaTime;
using Xs.Registry.Db.Shared.Models;

namespace Xs.Registry.Db.Shared.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession> CreateAsync(UserSession userSession);

    Task<UserSession> FindByTokenAsync(Guid token);

    Task ProlongateAsync(Guid token, Instant expires);

    Task DeleteByTokenAsync(Guid token);

    Task DeleteExpiredAsync(Instant now);
}