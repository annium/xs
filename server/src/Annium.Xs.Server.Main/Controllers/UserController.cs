using System;
using System.Threading.Tasks;
using Annium.Xs.Server.Main.Internal;
using Annium.Xs.Server.Main.Services;
using Annium.Xs.Server.Main.Views.Requests;
using Annium.Xs.Server.Shared.Auth;
using Annium.Xs.Server.Shared.Controllers;
using Annium.Xs.Server.Shared.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Annium.Xs.Server.Main.Controllers;

[Area(Constants.Project)]
[Route("[area]/user")]
public class UserController : ServerController<User>
{
    private readonly IUserService _userService;
    private readonly ISecurityService _securityService;

    public UserController(IUserService userService, ISecurityService securityService)
    {
        _userService = userService;
        _securityService = securityService;
    }

    [HttpPut]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserRegistrationRequest registrationModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var login = registrationModel.Login;

        if (await _userService.TryFindByNameAsync(login) is not null)
            return Conflict();

        var passwordHash = _securityService.Hash(registrationModel.Password);

        var user = new User(login, passwordHash, Guid.NewGuid());

        await _userService.CreateAsync(user);

        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UserUpdateRequest updateModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = GetUser();

        user.Update(updateModel.Login, _securityService.Hash(updateModel.Password), Guid.NewGuid());

        await _userService.UpdateAsync(user);

        return NoContent();
    }

    [HttpPost("token")]
    [Authorize]
    public async Task<IActionResult> UpdateUserApiTokenAsync()
    {
        var user = GetUser();

        var apiToken = Guid.NewGuid();

        await _userService.UpdateApiTokenAsync(user.Id, apiToken);

        return NoContent();
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUserAsync()
    {
        var user = GetUser();

        await _userService.DeleteByIdAsync(user.Id);

        return NoContent();
    }
}
