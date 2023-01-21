using System;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using LinqToDB;
using NodaTime;
using Xs.Registry.Db.Shared.Models;

namespace Xs.Registry.Db.Shared.Repositories;

internal class UserSessionRepository : IUserSessionRepository
{
    private readonly ISharedContext _context;

    private readonly IMapper _mapper;

    public UserSessionRepository(
        ISharedContext context,
        IMapper mapper
    )
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserSession> CreateAsync(UserSession userSession)
    {
        var entity = _mapper.Map<Entities.UserSession>(userSession);

        await _context.UserSessions
            .InsertAsync(() => new Entities.UserSession
            {
                UserId = entity.UserId,
                Token = entity.Token,
                Expires = entity.Expires,
            });

        return _mapper.Map<UserSession>(entity);
    }

    public async Task<UserSession> FindByTokenAsync(Guid token)
    {
        var entity = await _context.UserSessions.FirstOrDefaultAsync(u => u.Token == token);

        return _mapper.Map<UserSession>(entity);
    }

    public Task ProlongateAsync(Guid token, Instant expires)
    {
        var expiresDate = _mapper.Map<DateTime>(expires);

        return _context.UserSessions
            .UpdateAsync(
                s => s.Token == token,
                s => new Entities.UserSession { Expires = expiresDate }
            );
    }

    public Task DeleteByTokenAsync(Guid token)
    {
        return _context.UserSessions.DeleteAsync(s => s.Token == token);
    }

    public Task DeleteExpiredAsync(Instant now)
    {
        var nowDate = _mapper.Map<DateTime>(now);

        return _context.UserSessions.DeleteAsync(s => s.Expires < nowDate);
    }
}