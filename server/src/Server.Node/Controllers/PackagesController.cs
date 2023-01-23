using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Node.Domain;
using Server.Node.Internal;
using Server.Node.Views.Requests;
using Server.Node.Views.Responses;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Node.Controllers;

[Area(Constants.Project)]
[Route("[area]/packages")]
public class PackagesController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackageRequest> _packageService;

    public PackagesController(
        IPackageService<Package, PackageDependency, PackageRequest> packageService
    )
    {
        _packageService = packageService;
    }

    [HttpGet("{name}")]
    [Authorize]
    public async Task<IActionResult> GetPackagesAsync(string name)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.GetPackagesAsync(GetUser(), name);

        return result.Status switch
        {
            PackageStatus.Forbidden => new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden },
            PackageStatus.Ok        => Ok(result.Data.Select(p => new PackageResponse(p)).ToArray()),
            _                       => NotFound()
        };
    }

    [HttpDelete("{name}/{version}")]
    [Authorize]
    public async Task<IActionResult> DeletePackageAsync(string name, string version)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.UnpublishPackageAsync(GetUser(), name, version);

        return result.Status switch
        {
            PackageStatus.NotFound  => NotFound(),
            PackageStatus.Forbidden => new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden },
            _                       => NoContent()
        };
    }
}