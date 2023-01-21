using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Packages;
using Server.Db.Dotnet.Models;
using Server.Domain.Models;
using Server.Dotnet.Payloads;
using Server.Dotnet.Views;
using Server.Shared.Auth;
using Server.Shared.Helpers;

namespace Server.Dotnet.Controllers;

[Route("packages")]
public class PackagesController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackagePayload> _packageService;

    public PackagesController(
        IPackageService<Package, PackageDependency, PackagePayload> packageService,
        IMediator mediator,
        IServiceProvider sp
    ) : base(mediator, sp)
    {
        _packageService = packageService;
    }

    [HttpGet("{name}")]
    [AuthorizeApi]
    public async Task<IActionResult> GetPackagesAsync(string name)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.GetPackagesAsync(GetUser(), name);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            case PackageStatus.Ok:
                return Ok(result.Data.Select(p => new PackageView(p)).ToArray());
            default:
                return NotFound();
        }
    }

    [HttpDelete("{name}/{version}")]
    [AuthorizeApi]
    public async Task<IActionResult> DeletePackageAsync(string name, string version)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.UnpublishPackageAsync(GetUser(), name, version);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            default:
                return NoContent();
        }
    }
}