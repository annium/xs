using System;
using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Db
{
    public interface IUserRepository
    {
        Task CreateAsync(User user);

        Task<User> GetById(Guid id);

        Task<User> FindByNameAsync(string name);

        Task<User> FindByApiTokenAsync(Guid token);

        Task UpdateAsync(User user);

        Task UpdateApiTokenAsync(Guid userId, Guid apiToken);

        Task DeleteByNameAsync(string name);
    }
}