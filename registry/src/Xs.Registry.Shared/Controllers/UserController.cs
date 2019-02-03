using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Db;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Models;
using Xs.Registry.Core.Security;
using Xs.Registry.Shared.Payloads;

namespace Xs.Registry.Shared.Controllers
{
    [Route("user")]
    public class UserController : ServerController
    {
        private readonly ISecurityManager securityManager;

        private readonly IUserRepository userRepository;

        private readonly ILogger<UserController> logger;

        public UserController(
            ISecurityManager securityManager,
            IUserRepository userRepository,
            ILogger<UserController> logger
        )
        {
            this.securityManager = securityManager;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserRegistrationPayload registrationModel)
        {
            if (registrationModel == null)
                return BadRequest("Specify user data");

            if (!ModelState.IsValid)
                return BadRequest("Check user data");

            var name = registrationModel.Name;

            if (await userRepository.FindByNameAsync(name) != null)
                return Conflict();

            var passwordHash = securityManager.Hash(registrationModel.Password);
            var apiToken = Guid.NewGuid();

            var user = new User(name, passwordHash, apiToken);

            await userRepository.CreateAsync(user);

            return Ok(apiToken);
        }

        [HttpPost]
        [Authorize(Access.Session)]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UserUpdatePayload updateModel)
        {
            if (updateModel == null)
                return BadRequest("Specify user data");

            if (!ModelState.IsValid)
                return BadRequest("Check user data");

            var user = GetUser();

            user.Name = updateModel.Name;
            user.PasswordHash = securityManager.Hash(updateModel.Password);
            user.ApiToken = Guid.NewGuid();

            await userRepository.UpdateAsync(user);

            return Ok(user.ApiToken);
        }

        [HttpPost("token")]
        [Authorize(Access.Session)]
        public async Task<IActionResult> UpdateUserApiTokenAsync()
        {
            var user = GetUser();

            var apiToken = Guid.NewGuid();

            await userRepository.UpdateApiTokenAsync(user.Id, apiToken);

            return Ok(apiToken);
        }

        [HttpDelete]
        [Authorize(Access.Session)]
        public async Task<IActionResult> DeleteUserAsync()
        {
            var user = GetUser();

            await userRepository.DeleteByNameAsync(user.Name);

            return NoContent();
        }
    }
}