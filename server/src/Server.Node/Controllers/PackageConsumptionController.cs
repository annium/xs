using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Packages;
using Server.Db.Node.Models;
using Server.Domain.Models;
using Server.Node.Models;
using Server.Node.Payloads;
using Server.Node.Views;
using Server.Shared.Auth;
using Server.Shared.Controllers;
using Server.Shared.Extensions;
using IPackageStorage = Server.Node.Storage.IPackageStorage;

namespace Server.Node.Controllers;

public class PackageConsumptionController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackagePayload> _packageService;

    private readonly IPackageStorage _packageStorage;

    private readonly IUrlHelper _url;

    public PackageConsumptionController(
        IPackageService<Package, PackageDependency, PackagePayload> packageService,
        IPackageStorage packageStorage,
        IUrlHelper url,
        IMediator mediator,
        IServiceProvider sp
    ) : base(mediator, sp)
    {
        _packageService = packageService;
        _packageStorage = packageStorage;
        _url = url;
    }

    [HttpGet("{name}")]
    [AuthorizeApi]
    public async Task<IActionResult> GetPackageAsync([FromRoute] string name)
    {
        var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));
        var result = await _packageService.GetPackagesAsync(GetUser(), packageName);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            case PackageStatus.Ok:
                return Ok(new PackagesView(result.Data, _url));
            default:
                return NotFound();
        }
    }

    [HttpGet("{name}/{version}.tgz")]
    [AuthorizeApi]
    public async Task<IActionResult> DownloadPackageAsync([FromRoute] string name, [FromRoute] string version)
    {
        var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));
        var result = await _packageService.ProcessDownloadAsync(GetUser(), packageName, version, true);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            case PackageStatus.InternalError:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.InternalServerError };
        }

        var content = await _packageStorage.GetAsync(packageName, version);

        return File(content, MediaTypeNames.Application.Octet);
    }
}