using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Tools;
using Server.Dotnet.Internal;
using Server.Dotnet.Views.Responses;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Dotnet.Controllers;

[Area(Constants.Project)]
[Route("[area]")]
public class IndexController : ServerController<User>
{
    private readonly IUrlTool _urlTool;

    public IndexController(IServiceProvider sp)
    {
        _urlTool = sp.ResolveKeyed<IUrlTool>(Constants.ProjectType);
    }

    [HttpGet("v3/index.json")]
    public IActionResult GetIndexAsync()
    {
        var resources = new List<ServiceIndexResourceResponse>();

        resources.Add(new ServiceIndexResourceResponse(Uri("api/v2/package"), "PackagePublish/2.0.0"));
        resources.Add(new ServiceIndexResourceResponse(Uri("api/v2/symbol"), "SymbolPackagePublish/4.9.0"));
        resources.Add(new ServiceIndexResourceResponse(Uri("v3/registration"), "RegistrationsBaseUrl"));
        resources.Add(new ServiceIndexResourceResponse(Uri("v3/registration"), "RegistrationsBaseUrl/3.0.0-beta"));
        resources.Add(new ServiceIndexResourceResponse(Uri("v3/registration"), "RegistrationsBaseUrl/3.0.0-rc"));
        resources.Add(new ServiceIndexResourceResponse(Uri("v3/registration"), "RegistrationsBaseUrl/3.4.0"));
        resources.Add(new ServiceIndexResourceResponse(Uri("v3/registration"), "RegistrationsBaseUrl/3.6.0"));
        resources.Add(new ServiceIndexResourceResponse(Uri("v3/registration"), "RegistrationsBaseUrl/Versioned"));
        resources.Add(new ServiceIndexResourceResponse(Uri("v3/package"), "PackageBaseAddress"));

        return Ok(new { version = "3.0.0", resources });

        Uri Uri(string relative) => _urlTool.AbsoluteUrl(relative);
    }
}
