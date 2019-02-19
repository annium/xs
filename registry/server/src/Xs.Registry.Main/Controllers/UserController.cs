using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xs.Registry.Db.Shared;
using Xs.Registry.Main.Payloads;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Controllers
{
    [Route("user")]
    public class UserController : ServerController<User>
    {
        private readonly IUserRepository userRepository;

        private readonly ISecurityManager securityManager;

        private readonly ILogger<UserController> logger;

        public UserController(
            IUserRepository userRepository,
            ISecurityManager securityManager,
            ILogger<UserController> logger
        )
        {
            this.userRepository = userRepository;
            this.securityManager = securityManager;
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

            var user = new User(name, passwordHash, Guid.NewGuid());

            await userRepository.CreateAsync(user);

            return NoContent();
        }

        [HttpPost]
        [AuthorizeSession]
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

            return NoContent();
        }

        [HttpPost("token")]
        [AuthorizeSession]
        public async Task<IActionResult> UpdateUserApiTokenAsync()
        {
            var user = GetUser();

            var apiToken = Guid.NewGuid();

            await userRepository.UpdateApiTokenAsync(user.Id, apiToken);

            return NoContent();
        }

        [HttpDelete]
        [AuthorizeSession]
        public async Task<IActionResult> DeleteUserAsync()
        {
            var user = GetUser();

            await userRepository.DeleteByIdAsync(user.Id);

            return NoContent();
        }
    }
}