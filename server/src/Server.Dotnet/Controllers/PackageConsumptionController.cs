using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Dotnet.Domain;
using Server.Dotnet.Internal;
using Server.Dotnet.Services;
using Server.Dotnet.Views.Requests;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Dotnet.Controllers;

[Area(Constants.Project)]
[Route("[area]")]
public class PackageConsumptionController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackageRequest> _packageService;
    private readonly IPackageStorage _packageStorage;

    public PackageConsumptionController(
        IPackageService<Package, PackageDependency, PackageRequest> packageService,
        IPackageStorage packageStorage
    )
    {
        _packageService = packageService;
        _packageStorage = packageStorage;
    }

    [HttpGet("v3/package/{name}/index.json")]
    public async Task<IActionResult> GetVersionsAsync(string name)
    {
        name = HttpUtility.UrlDecode(name);
        var versions = await _packageService.FindAllByNameAsync(name);

        if (versions.Count == 0)
            return NotFound();

        return Ok(new { versions = versions.Select(x => x.Version).ToArray() });
    }

    [HttpGet("v3/package/{name}/{version}/{name2}.{version2}.nupkg")]
    public async Task<IActionResult> DownloadPackageAsync(string name, string version)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.ProcessDownloadAsync(null, name, version, true);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden };
            case PackageStatus.InternalError:
                return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }

        var content = await _packageStorage.GetPackageAsync(name, version);

        return File(content, "application/octet-stream");
    }

    [HttpGet("v3/package/{name}/{version}/{name2}.nuspec")]
    public async Task<IActionResult> DownloadNuspecAsync(string name, string version)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.ProcessDownloadAsync(null, name, version, false);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden };
            case PackageStatus.InternalError:
                return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }

        var content = await _packageStorage.GetNuspecAsync(name, version);

        return File(content, "text/xml");
    }
}