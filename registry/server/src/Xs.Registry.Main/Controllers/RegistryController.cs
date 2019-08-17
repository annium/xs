using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Shared;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Controllers
{
    [Route("registry")]
    public class RegistryController : ServerController<User>
    {
        private readonly Configuration configuration;

        public RegistryController(
            Configuration configuration
        )
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetRegistries()
        {
            return Ok(configuration);
        }
    }
}