using System.Threading.Tasks;
using Xs.Core.Models;

namespace Xs.Registry.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User> FindByNameAsync(string name);

        Task<User> FindByTokenAsync(string token);

        Task SaveAsync(User user);

        Task DeleteByNameAsync(string name);
    }
}