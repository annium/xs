using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xs.Registry.Main.Payloads;

namespace Xs.Registry.Main.Controllers
{
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;

        public UserController(
            ILogger<UserController> logger
        )
        {
            this.logger = logger;
        }

        [HttpPut]
        public IActionResult CreateUserAsync([FromBody] UserRegistrationPayload registrationModel)
        {
            if (registrationModel == null)
                return BadRequest("Specify user data");

            if (!ModelState.IsValid)
                return BadRequest("Check user data");

            return Ok(Guid.NewGuid());
        }
    }
}