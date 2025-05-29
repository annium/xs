using System.Linq;
using Annium.Xs.Server.Main.Internal;
using Annium.Xs.Server.Shared.Controllers;
using Annium.Xs.Server.Shared.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Annium.Xs.Server.Main.Controllers;

[Area(Constants.Project)]
[Route("[area]/registry")]
public class RegistryController : ServerController<User>
{
    private readonly Shared.Configuration _configuration;

    public RegistryController(Shared.Configuration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetRegistries()
    {
        var response = new { servers = _configuration.Servers.ToDictionary(x => x.Key.ToString(), x => x.Value) };

        return Ok(response);
    }
}
