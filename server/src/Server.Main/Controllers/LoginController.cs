using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Domain.Models;
using Server.Main.Internal;
using Server.Main.Services;
using Server.Main.Views.Requests;
using Server.Shared.Controllers;

namespace Server.Main.Controllers;

[Area(Constants.Project)]
[Route("[area]/login")]
public class LoginController : ServerController<User>
{
    private readonly IUserService _userService;
    private readonly ISecurityService _securityService;
    private readonly IUserSessionService _userSessionService;

    public LoginController(
        IUserService userService,
        ISecurityService securityService,
        IUserSessionService userSessionService
    )
    {
        _userService = userService;
        _securityService = securityService;
        _userSessionService = userSessionService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginUserAsync([FromBody] UserLoginRequest? loginRequest)
    {
        var (user, result) = await LoginUserInternalAsync(loginRequest);
        if (result is not null)
            return result;

        await _userSessionService.CreateSession(user!.Id);

        return NoContent();
    }

    [HttpPost("app")]
    public async Task<IActionResult> LoginAppAsync([FromBody] UserLoginRequest? loginRequest)
    {
        var (user, result) = await LoginUserInternalAsync(loginRequest);
        if (result is not null)
            return result;

        return Ok(user!.ApiToken);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Info()
    {
        var user = GetUser();

        return Ok(new { user.Id, Name = user.Login, user.ApiToken });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        await _userSessionService.DeleteCurrentSession();

        return NoContent();
    }

    private async Task<ValueTuple<User?, IActionResult?>> LoginUserInternalAsync(UserLoginRequest? loginRequest)
    {
        if (loginRequest is null)
            return (null, BadRequest("Pass login data"));

        var name = loginRequest.Login;
        var password = loginRequest.Password;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            return (null, BadRequest("Pass login data"));

        var user = await _userService.TryFindByNameAsync(name);
        if (user is null)
            return (null, NotFound("User not found"));

        var passwordHash = _securityService.Hash(password);
        if (user.PasswordHash != passwordHash)
            return (null, new ObjectResult("Invalid password") { StatusCode = (int) HttpStatusCode.Forbidden });

        return (user, null);
    }
}