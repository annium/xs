using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Dotnet.Models;
using Xs.Registry.Db.Shared.Models;
using Xs.Registry.Db.Shared.Repositories;
using Xs.Registry.Dotnet.Payloads;
using Xs.Registry.Shared.Helpers;
using IPackageStorage = Xs.Registry.Dotnet.Storage.IPackageStorage;

namespace Xs.Registry.Dotnet.Controllers;

public class PackageConsumptionController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackagePayload> _packageService;

    private readonly IPackageRepository<Package, PackageDependency> _packageRepository;

    private readonly IPackageStorage _packageStorage;

    public PackageConsumptionController(
        IPackageService<Package, PackageDependency, PackagePayload> packageService,
        IPackageRepository<Package, PackageDependency> packageRepository,
        IPackageStorage packageStorage,
        IMediator mediator,
        IServiceProvider sp
    ) : base(mediator, sp)
    {
        _packageService = packageService;
        _packageRepository = packageRepository;
        _packageStorage = packageStorage;
    }

    [HttpGet("v3/package/{name}/index.json")]
    public async Task<IActionResult> GetVersionsAsync(string name, CancellationToken ct)
    {
        name = HttpUtility.UrlDecode(name);
        var versions = await _packageRepository.FindAllVersionsByNameAsync(name);

        if (versions.Length == 0)
            return NotFound();

        return Ok(new { versions });
    }

    [HttpGet("v3/package/{name}/{version}/{name2}.{version2}.nupkg")]
    public async Task<IActionResult> DownloadPackageAsync(string name, string version, CancellationToken ct)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.ProcessDownloadAsync(null, name, version, true);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            case PackageStatus.InternalError:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.InternalServerError };
        }

        var content = await _packageStorage.GetPackageAsync(name, version);

        return File(content, "application/octet-stream");
    }

    [HttpGet("v3/package/{name}/{version}/{name2}.nuspec")]
    public async Task<IActionResult> DownloadNuspecAsync(string name, string version, CancellationToken ct)
    {
        name = HttpUtility.UrlDecode(name);
        var result = await _packageService.ProcessDownloadAsync(null, name, version, false);
        switch (result.Status)
        {
            case PackageStatus.NotFound:
                return NotFound();
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            case PackageStatus.InternalError:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.InternalServerError };
        }

        var content = await _packageStorage.GetNuspecAsync(name, version);

        return File(content, "text/xml");
    }
}