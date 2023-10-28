using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Main.Internal;
using Server.Main.Services;
using Server.Main.Views.Requests;
using Server.Shared.Auth;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Main.Controllers;

[Area(Constants.Project)]
[Route("[area]/login")]
public class LoginController : ServerController<User>
{
    private readonly IUserService _userService;
    private readonly ISecurityService _securityService;

    public LoginController(IUserService userService, ISecurityService securityService)
    {
        _userService = userService;
        _securityService = securityService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginAppAsync([FromBody] UserLoginRequest? loginRequest)
    {
        if (loginRequest is null)
            return BadRequest("Pass login data");

        var name = loginRequest.Login;
        var password = loginRequest.Password;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Pass login data");

        var user = await _userService.TryFindByNameAsync(name);
        if (user is null)
            return NotFound("User not found");

        var passwordHash = _securityService.Hash(password);
        if (user.PasswordHash != passwordHash)
            return new ObjectResult("Invalid password") { StatusCode = (int)HttpStatusCode.Forbidden };

        return Ok(user.ApiToken);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Info()
    {
        var user = GetUser();

        return Ok(
            new
            {
                user.Id,
                Name = user.Login,
                user.ApiToken
            }
        );
    }
}
