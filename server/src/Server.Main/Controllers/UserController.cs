using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Domain.Models;
using Server.Main.Payloads;
using Server.Main.Services;
using Server.Main.Tools;
using Server.Shared.Auth.Attributes;
using Server.Shared.Controllers;

namespace Server.Main.Controllers;

[Route("user")]
public class UserController : ServerController<User>
{
    private readonly IUserService _userService;
    private readonly ISecurityManager _securityManager;

    public UserController(
        IUserService userService,
        ISecurityManager securityManager
    )
    {
        _userService = userService;
        _securityManager = securityManager;
    }

    [HttpPut]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserRegistrationPayload registrationModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var login = registrationModel.Login;

        if (await _userService.TryFindByNameAsync(login) is not null)
            return Conflict();

        var passwordHash = _securityManager.Hash(registrationModel.Password);

        var user = new User(login, passwordHash, Guid.NewGuid());

        await _userService.CreateAsync(user);

        return NoContent();
    }

    [HttpPost]
    [AuthorizeSession]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UserUpdatePayload updateModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = GetUser();

        user.Update(updateModel.Name, _securityManager.Hash(updateModel.Password), Guid.NewGuid());

        await _userService.UpdateAsync(user);

        return NoContent();
    }

    [HttpPost("token")]
    [AuthorizeSession]
    public async Task<IActionResult> UpdateUserApiTokenAsync()
    {
        var user = GetUser();

        var apiToken = Guid.NewGuid();

        await _userService.UpdateApiTokenAsync(user.Id, apiToken);

        return NoContent();
    }

    [HttpDelete]
    [AuthorizeSession]
    public async Task<IActionResult> DeleteUserAsync()
    {
        var user = GetUser();

        await _userService.DeleteByIdAsync(user.Id);

        return NoContent();
    }
}