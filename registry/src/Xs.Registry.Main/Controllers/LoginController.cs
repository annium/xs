using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IUserRepository userRepository;

        private readonly ISecurityManager securityManager;

        private readonly ISessionManager sessionManager;

        private readonly ILogger<LoginController> logger;

        public LoginController(
            IUserRepository userRepository,
            ISecurityManager securityManager,
            ISessionManager sessionManager,
            ILogger<LoginController> logger
        )
        {
            this.userRepository = userRepository;
            this.securityManager = securityManager;
            this.sessionManager = sessionManager;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> LoginUserAsync([FromBody] UserLoginPayload loginPayload)
        {
            var(user, result) = await LoginUserInternalAsync(loginPayload);
            if (result != null)
                return result;

            await sessionManager.CreateSession(user.Id);

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
        [Authorize]
        public IActionResult Info()
        {
            var user = GetUser();

            return Ok(new { Id = user.Id, Name = user.Name, ApiToken = user.ApiToken });
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> LogoutAsync()
        {
            await sessionManager.DeleteCurrentSession();

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

            var user = await userRepository.FindByNameAsync(name);
            if (user == null)
                return (null, NotFound("User not found"));

            var passwordHash = securityManager.Hash(password);
            if (user.PasswordHash != passwordHash)
                return (null, Forbidden("Invalid password"));

            return (user, null);
        }
    }
}