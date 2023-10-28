using System;
using System.Threading.Tasks;
using Server.Main.Services;
using Server.Shared.Domain.Models;
using Server.Shared.Repositories;

namespace Server.Main.Internal.Services;

internal class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task CreateAsync(User user) => _repository.CreateAsync(user);

    public Task<User?> TryFindByNameAsync(string name) => _repository.TryFindByLoginAsync(name);

    public Task UpdateAsync(User user) => _repository.UpdateAsync(user);

    public Task UpdateApiTokenAsync(Guid userId, Guid apiToken) => _repository.UpdateApiTokenAsync(userId, apiToken);

    public Task DeleteByIdAsync(Guid id) => _repository.DeleteByIdAsync(id);
}
