using System;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Shared.Models;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Controllers;

[Route("registry")]
public class RegistryController : ServerController<User>
{
    private readonly Configuration _configuration;

    public RegistryController(
        Configuration configuration,
        IMediator mediator,
        IServiceProvider sp
    ) : base(mediator, sp)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetRegistries()
    {
        return Ok(_configuration);
    }
}