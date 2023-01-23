using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Domain.Models;
using Server.Dotnet.Domain;
using Server.Dotnet.Internal;
using Server.Dotnet.Views.Requests;
using Server.Dotnet.Views.Responses;
using Server.Shared.Auth.Attributes;
using Server.Shared.Controllers;

namespace Server.Dotnet.Controllers;

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
    [AuthorizeApi]
    public async Task<IActionResult> GetPackagesAsync(string name)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.GetPackagesAsync(GetUser(), name);

        return result.Status switch
        {
            PackageStatus.NotFound => NotFound(),
            PackageStatus.Forbidden => new ObjectResult(result)
            {
                StatusCode = (int) HttpStatusCode.Forbidden
            },
            PackageStatus.Ok => Ok(result.Data
                .Select(p => new PackageResponse(
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
            _ => NotFound()
        };
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