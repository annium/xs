using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.linq2db.Extensions;
using LinqToDB;
using Server.Shared.Domain.Models;
using Server.Shared.Repositories;

namespace Server.Shared.Internal.Repositories;

internal class UserRepository : RepositoryBase<Connection>, IUserRepository
{
    public UserRepository(Connection db)
        : base(db) { }

    public async Task CreateAsync(User user)
    {
        await Db.Users.InsertAsync(user);
    }

    public async Task<User?> TryFindByLoginAsync(string login)
    {
        return await Db.Users.FirstOrDefaultAsync(x => x.Login == login);
    }

    public async Task<User?> TryFindByApiTokenAsync(Guid token)
    {
        return await Db.Users.FirstOrDefaultAsync(x => x.ApiToken == token);
    }

    public async Task UpdateAsync(User user)
    {
        await Db.Users.UpdateAsync(user);
    }

    public async Task UpdateApiTokenAsync(Guid userId, Guid apiToken)
    {
        await Db.Users.Where(x => x.Id == userId).Set(x => x.ApiToken, apiToken).UpdateAsync();
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        await Db.Users.DeleteAsync(x => x.Id == id);
    }
}
