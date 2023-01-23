using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.linq2db.Extensions.Extensions;
using LinqToDB;
using NodaTime;
using Server.Domain.Models;
using Server.Shared.Repositories;

namespace Server.Shared.Internal.Repositories;

internal class UserSessionRepository : RepositoryBase<Connection>, IUserSessionRepository
{
    private readonly ITimeProvider _timeProvider;

    public UserSessionRepository(
        ITimeProvider timeProvider,
        Connection db
    ) : base(db)
    {
        _timeProvider = timeProvider;
    }

    public async Task CreateAsync(UserSession userSession)
    {
        await Db.UserSessions.InsertAsync(userSession);
    }

    public async Task<UserSession?> TryFindByTokenAsync(Guid token)
    {
        return await Db.UserSessions.SingleOrDefaultAsync(x => x.Token == token);
    }

    public async Task ExtendAsync(Guid token, Instant expires)
    {
        await Db.UserSessions
            .Where(x => x.Token == token)
            .Set(x => x.Expires, expires)
            .UpdateAsync();
    }

    public async Task DeleteByTokenAsync(Guid token)
    {
        await Db.UserSessions.DeleteAsync(x => x.Token == token);
    }

    public async Task DeleteExpiredAsync()
    {
        var now = _timeProvider.Now;

        await Db.UserSessions.DeleteAsync(x => x.Expires < now);
    }
}