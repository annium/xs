using System;
using System.Threading.Tasks;
using Server.Shared.Domain.Models;

namespace Server.Main.Services;

public interface IUserService
{
    Task CreateAsync(User user);
    Task<User?> TryFindByNameAsync(string name);
    Task UpdateAsync(User user);
    Task UpdateApiTokenAsync(Guid userId, Guid apiToken);
    Task DeleteByIdAsync(Guid id);
}