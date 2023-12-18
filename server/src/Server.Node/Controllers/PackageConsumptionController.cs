using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Abstractions.Tools;
using Server.Node.Domain;
using Server.Node.Internal;
using Server.Node.Services;
using Server.Node.Views.Requests;
using Server.Node.Views.Responses;
using Server.Shared.Auth;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Node.Controllers;

[Area(Constants.Project)]
[Route("[area]")]
public class PackageConsumptionController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackageRequest> _packageService;
    private readonly IPackageStorage _packageStorage;
    private readonly IUrlTool _urlTool;

    public PackageConsumptionController(
        IServiceProvider sp,
        IPackageService<Package, PackageDependency, PackageRequest> packageService,
        IPackageStorage packageStorage
    )
    {
        _packageService = packageService;
        _packageStorage = packageStorage;
        _urlTool = sp.ResolveKeyed<IUrlTool>(Constants.ProjectType);
    }

    [HttpGet("{name}")]
    [Authorize]
    public async Task<IActionResult> GetPackageAsync([FromRoute] string name)
    {
        var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));
        var result = await _packageService.GetPackagesAsync(GetUser(), packageName);

        return result.Status switch
        {
            PackageStatus.NotFound => NotFound(),
            PackageStatus.Forbidden => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden },
            PackageStatus.Ok => Ok(new PackagesResponse(result.Data, _urlTool)),
            _ => NotFound()
        };
    }

    [HttpGet("{name}/{version}.tgz")]
    [Authorize]
    public async Task<IActionResult> DownloadPackageAsync([FromRoute] string name, [FromRoute] string version)
    {
        var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));
        var result = await _packageService.ProcessDownloadAsync(GetUser(), packageName, version, true);

        return result.Status switch
        {
            PackageStatus.NotFound => NotFound(),
            PackageStatus.Forbidden => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden },
            PackageStatus.InternalError
                => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.InternalServerError },
            PackageStatus.Ok
                => File(await _packageStorage.GetAsync(packageName, version), MediaTypeNames.Application.Octet),
            PackageStatus.Conflict => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.InternalServerError },
            _ => new ObjectResult(result) { StatusCode = (int)HttpStatusCode.InternalServerError }
        };
    }
}
