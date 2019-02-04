using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xs.Registry.Db.Shared;
using Xs.Registry.Main.Payloads;
using Xs.Registry.Main.Tools;
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

            return Ok();
        }
    }
}