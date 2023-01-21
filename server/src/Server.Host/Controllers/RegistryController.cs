using System;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Server.Shared.Helpers;
using Xs.Registry.Db.Shared.Models;

namespace Server.Host.Controllers;

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