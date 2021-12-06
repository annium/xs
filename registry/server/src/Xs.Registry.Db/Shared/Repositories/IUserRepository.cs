using System;
using System.Threading.Tasks;

namespace Xs.Registry.Db.Shared;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);

    Task<User> GetById(Guid id);

    Task<User> FindByNameAsync(string name);

    Task<User> FindByApiTokenAsync(Guid token);

    Task UpdateAsync(User user);

    Task UpdateApiTokenAsync(Guid userId, Guid apiToken);

    Task DeleteByIdAsync(Guid id);
}