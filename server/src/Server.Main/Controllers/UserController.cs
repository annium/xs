using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Main.Internal;
using Server.Main.Services;
using Server.Main.Views.Requests;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Main.Controllers;

[Area(Constants.Project)]
[Route("[area]/user")]
public class UserController : ServerController<User>
{
    private readonly IUserService _userService;
    private readonly ISecurityService _securityService;

    public UserController(
        IUserService userService,
        ISecurityService securityService
    )
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