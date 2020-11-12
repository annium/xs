using System;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Shared;
using Xs.Registry.Main.Auth;
using Xs.Registry.Main.Payloads;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Controllers
{
    [Route("login")]
    public class LoginController : ServerController<User>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityManager _securityManager;
        private readonly ISessionManager _sessionManager;

        public LoginController(
            IUserRepository userRepository,
            ISecurityManager securityManager,
            ISessionManager sessionManager,
            IMediator mediator
        ) : base(mediator)
        {
            _userRepository = userRepository;
            _securityManager = securityManager;
            _sessionManager = sessionManager;
        }

        [HttpPost]
        public async Task<IActionResult> LoginUserAsync([FromBody] UserLoginPayload loginPayload)
        {
            var(user, result) = await LoginUserInternalAsync(loginPayload);
            if (result != null)
                return result;

            await _sessionManager.CreateSession(user.Id);

            return NoContent();
        }

        [HttpPost("app")]
        public async Task<IActionResult> LoginAppAsync([FromBody] UserLoginPayload loginPayload)
        {
            var(user, result) = await LoginUserInternalAsync(loginPayload);
            if (result != null)
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
            if (loginPayload == null)
                return (null, BadRequest("Pass login data"));

            var name = loginPayload.Name;
            var password = loginPayload.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                return (null, BadRequest("Pass login data"));

            var user = await _userRepository.FindByNameAsync(name);
            if (user == null)
                return (null, NotFound("User not found"));

            var passwordHash = _securityManager.Hash(password);
            if (user.PasswordHash != passwordHash)
                return (null, Forbidden("Invalid password"));

            return (user, null);
        }
    }
}