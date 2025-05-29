using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Dotnet.Domain;
using Annium.Xs.Server.Dotnet.Internal;
using Annium.Xs.Server.Dotnet.Views.Requests;
using Annium.Xs.Server.Dotnet.Views.Responses;
using Annium.Xs.Server.Shared.Auth;
using Annium.Xs.Server.Shared.Controllers;
using Annium.Xs.Server.Shared.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Annium.Xs.Server.Dotnet.Controllers;

[Area(Constants.Project)]
[Route("[area]/packages")]
public class PackagesController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackageRequest> _packageService;

    public PackagesController(IPackageService<Package, PackageDependency, PackageRequest> packageService)
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
            PackageStatus.NotFound => NotFound(),
            PackageStatus.Forbidden => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden },
            PackageStatus.Ok => Ok(
                result
                    .Data.Select(p => new PackageResponse(
                        p.Id,
                        p.Name,
                        p.Version,
                        p.Description,
                        p.Published,
                        p.Downloads,
                        p.Dependencies
                    ))
                    .ToArray()
            ),
            _ => NotFound(),
        };
    }

    [HttpDelete("{name}/{version}")]
    [Authorize]
    public async Task<IActionResult> DeletePackageAsync(string name, string version)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.UnpublishPackageAsync(GetUser(), name, version);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden };
            default:
                return NoContent();
        }
    }
}
