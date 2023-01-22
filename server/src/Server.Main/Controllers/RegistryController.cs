using System;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Server.Domain.Models;
using Server.Shared.Controllers;
using Server.Shared.Extensions;

namespace Server.Main.Controllers;

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