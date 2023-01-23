using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Domain.Models;
using Server.Node.Domain;
using Server.Node.Internal.Services;
using Server.Node.Views;
using Server.Node.Views.Requests;
using Server.Node.Views.Responses;
using Server.Shared.Auth.Attributes;
using Server.Shared.Controllers;

namespace Server.Node.Controllers;

[Route("packages")]
public class PackagesController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackagePackageRequest> _packageService;

    public PackagesController(
        IPackageService<Package, PackageDependency, PackagePackageRequest> packageService
    )
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
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            case PackageStatus.Ok:
                return Ok(result.Data.Select(p => new PackageResponse(p)).ToArray());
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