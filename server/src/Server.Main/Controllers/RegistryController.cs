using Microsoft.AspNetCore.Mvc;
using Server.Domain.Models;
using Server.Shared.Controllers;

namespace Server.Main.Controllers;

[Route("registry")]
public class RegistryController : ServerController<User>
{
    private readonly Configuration _configuration;

    public RegistryController(
        Configuration configuration
    )
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetRegistries()
    {
        return Ok(_configuration);
    }
}