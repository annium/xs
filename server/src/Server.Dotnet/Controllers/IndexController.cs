using System;
using System.Collections.Generic;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Server.Domain.Models;
using Server.Dotnet.Views;
using Server.Shared.Helpers;

namespace Server.Dotnet.Controllers;

public class IndexController : ServerController<User>
{
    private readonly IUrlHelper _url;

    public IndexController(
        IUrlHelper url,
        IMediator mediator,
        IServiceProvider sp
    ) : base(mediator, sp)
    {
        _url = url;
    }

    [HttpGet("v3/index.json")]
    public IActionResult GetIndexAsync()
    {
        var resources = new List<ServiceIndexResourceView>();

        resources.Add(new ServiceIndexResourceView { Type = "PackagePublish/2.0.0", Uri = _url.AbsoluteUri("api/v2/package") });
        resources.Add(new ServiceIndexResourceView { Type = "SymbolPackagePublish/4.9.0", Uri = _url.AbsoluteUri("api/v2/symbol") });
        resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl", Uri = _url.AbsoluteUri("v3/registration") });
        resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl/3.0.0-beta", Uri = _url.AbsoluteUri("v3/registration") });
        resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl/3.0.0-rc", Uri = _url.AbsoluteUri("v3/registration") });
        resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl/3.4.0", Uri = _url.AbsoluteUri("v3/registration") });
        resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl/3.6.0", Uri = _url.AbsoluteUri("v3/registration") });
        resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl/Versioned", Uri = _url.AbsoluteUri("v3/registration") });
        resources.Add(new ServiceIndexResourceView { Type = "PackageBaseAddress", Uri = _url.AbsoluteUri("v3/package") });

        return Ok(new { version = "3.0.0", resources });
    }
}