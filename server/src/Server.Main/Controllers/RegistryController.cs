using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Server.Main.Internal;
using Server.Shared;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Main.Controllers;

[Area(Constants.Project)]
[Route("[area]/registry")]
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
        var response = new
        {
            servers = _configuration.Servers.ToDictionary(x => x.Key.ToString(), x => x.Value)
        };

        return Ok(response);
    }
}