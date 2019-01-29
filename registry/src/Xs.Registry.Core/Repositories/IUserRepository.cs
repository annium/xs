using System;
using System.Threading.Tasks;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User[]> GetByIdsAsync(string[] ids);

        Task<User> GetByIdAsync(string id);

        Task<User> FindByNameAsync(string name);

        Task<User> FindByApiTokenAsync(Guid token);

        Task<User> FindBySessionTokenAsync(Guid token);

        Task SaveAsync(User user);

        Task DeleteByNameAsync(string name);
    }
}