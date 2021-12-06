using System;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Shared;
using Xs.Registry.Main.Payloads;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Controllers;

[Route("user")]
public class UserController : ServerController<User>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityManager _securityManager;

    public UserController(
        IUserRepository userRepository,
        ISecurityManager securityManager,
        IMediator mediator,
        IServiceProvider sp
    ) : base(mediator, sp)
    {
        _userRepository = userRepository;
        _securityManager = securityManager;
    }

    [HttpPut]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserRegistrationPayload registrationModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var name = registrationModel.Name;

        if (await _userRepository.FindByNameAsync(name) != null)
            return Conflict();

        var passwordHash = _securityManager.Hash(registrationModel.Password);

        var user = new User(name, passwordHash, Guid.NewGuid());

        await _userRepository.CreateAsync(user);

        return NoContent();
    }

    [HttpPost]
    [AuthorizeSession]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UserUpdatePayload updateModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = GetUser();

        user.Name = updateModel.Name;
        user.PasswordHash = _securityManager.Hash(updateModel.Password);
        user.ApiToken = Guid.NewGuid();

        await _userRepository.UpdateAsync(user);

        return NoContent();
    }

    [HttpPost("token")]
    [AuthorizeSession]
    public async Task<IActionResult> UpdateUserApiTokenAsync()
    {
        var user = GetUser();

        var apiToken = Guid.NewGuid();

        await _userRepository.UpdateApiTokenAsync(user.Id, apiToken);

        return NoContent();
    }

    [HttpDelete]
    [AuthorizeSession]
    public async Task<IActionResult> DeleteUserAsync()
    {
        var user = GetUser();

        await _userRepository.DeleteByIdAsync(user.Id);

        return NoContent();
    }
}