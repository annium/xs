using System;
using System.Threading.Tasks;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Shared.Repositories;

internal interface IUserRepository
{
    Task CreateAsync(User user);
    Task<User?> TryFindByLoginAsync(string login);
    Task<User?> TryFindByApiTokenAsync(Guid token);
    Task UpdateAsync(User user);
    Task UpdateApiTokenAsync(Guid userId, Guid apiToken);
    Task DeleteByIdAsync(Guid id);
}
