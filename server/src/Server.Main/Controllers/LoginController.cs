using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Db.Repositories;
using Server.Domain.Models;
using Server.Main.Auth;
using Server.Main.Payloads;
using Server.Main.Tools;
using Server.Shared.Auth;
using Server.Shared.Controllers;

namespace Server.Main.Controllers;

[Route("login")]
public class LoginController : ServerController<User>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityManager _securityManager;
    private readonly ISessionManager _sessionManager;

    public LoginController(
        IUserRepository userRepository,
        ISecurityManager securityManager,
        ISessionManager sessionManager
    )
    {
        _userRepository = userRepository;
        _securityManager = securityManager;
        _sessionManager = sessionManager;
    }

    [HttpPost]
    public async Task<IActionResult> LoginUserAsync([FromBody] UserLoginPayload loginPayload)
    {
        var (user, result) = await LoginUserInternalAsync(loginPayload);
        if (result is not null)
            return result;

        await _sessionManager.CreateSession(user.Id);

        return NoContent();
    }

    [HttpPost("app")]
    public async Task<IActionResult> LoginAppAsync([FromBody] UserLoginPayload loginPayload)
    {
        var (user, result) = await LoginUserInternalAsync(loginPayload);
        if (result is not null)
            return result;

        return Ok(user.ApiToken);
    }

    [HttpGet]
    [AuthorizeSession]
    public IActionResult Info()
    {
        var user = GetUser();

        return Ok(new { Id = user.Id, Name = user.Name, ApiToken = user.ApiToken });
    }

    [HttpDelete]
    [AuthorizeSession]
    public async Task<IActionResult> LogoutAsync()
    {
        await _sessionManager.DeleteCurrentSession();

        return NoContent();
    }

    private async Task<ValueTuple<User, IActionResult>> LoginUserInternalAsync(UserLoginPayload loginPayload)
    {
        if (loginPayload is null)
            return (null, BadRequest("Pass login data"));

        var name = loginPayload.Name;
        var password = loginPayload.Password;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            return (null, BadRequest("Pass login data"));

        var user = await _userRepository.FindByNameAsync(name);
        if (user is null)
            return (null, NotFound("User not found"));

        var passwordHash = _securityManager.Hash(password);
        if (user.PasswordHash != passwordHash)
            return (null, new ObjectResult("Invalid password") { StatusCode = (int) HttpStatusCode.Forbidden });

        return (user, null);
    }
}