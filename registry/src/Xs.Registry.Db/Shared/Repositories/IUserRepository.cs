using System.Threading.Tasks;

namespace Xs.Registry.Db.Shared
{
    public interface IUserRepository
    {
        Task CreateAsync(User user);

        Task<User> FindByNameAsync(string name);
    }
}