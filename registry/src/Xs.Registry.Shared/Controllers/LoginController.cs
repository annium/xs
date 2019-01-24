using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Models;
using Xs.Registry.Core.Repositories;
using Xs.Registry.Core.Security;

namespace Xs.Registry.Shared.Controllers
{
    [Route("login")]
    public class LoginController : ServerController
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

        [HttpPost("app")]
        public async Task<IActionResult> LoginAppAsync(string name, string password)
        {
            var(user, result) = await LoginUserInternalAsync(name, password);
            if (result != null)
                return result;

            return Ok(user.ApiToken);
        }

        private async Task<ValueTuple<User, IActionResult>> LoginUserInternalAsync(string name, string password)
        {
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