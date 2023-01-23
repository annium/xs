using System;
using System.Threading.Tasks;
using NodaTime;
using Server.Domain.Models;

namespace Server.Shared.Repositories;

internal interface IUserSessionRepository
{
    Task CreateAsync(UserSession userSession);
    Task<UserSession?> TryFindByTokenAsync(Guid token);
    Task ExtendAsync(Guid token, Instant expires);
    Task DeleteByTokenAsync(Guid token);
    Task DeleteExpiredAsync();
}