using System;
using System.Threading.Tasks;
using Annium.Xs.Server.Main.Services;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Repositories;

namespace Annium.Xs.Server.Main.Internal.Services;

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
