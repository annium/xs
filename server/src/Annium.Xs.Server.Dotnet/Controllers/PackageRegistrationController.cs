using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Abstractions.Tools;
using Annium.Xs.Server.Dotnet.Domain;
using Annium.Xs.Server.Dotnet.Internal;
using Annium.Xs.Server.Dotnet.Views.Requests;
using Annium.Xs.Server.Dotnet.Views.Responses;
using Annium.Xs.Server.Shared.Controllers;
using Annium.Xs.Server.Shared.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Annium.Xs.Server.Dotnet.Controllers;

[Area(Constants.Project)]
[Route("[area]")]
public class PackageRegistrationController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackageRequest> _packageService;
    private readonly IUrlTool _urlTool;

    public PackageRegistrationController(
        IServiceProvider sp,
        IPackageService<Package, PackageDependency, PackageRequest> packageService
    )
    {
        _packageService = packageService;
        _urlTool = sp.ResolveKeyed<IUrlTool>(Constants.ProjectType);
    }

    [HttpGet("v3/registration/{name}/index.json")]
    public async Task<IActionResult> GetRegistrationIndexAsync(string name)
    {
        var packages = await _packageService.FindAllByNameAsync(name);

        if (packages.Count == 0)
            return NotFound();

        return Ok(new RegistrationIndexResponse([GetRegistrationPage(packages)]));
    }

    [HttpGet("v3/registration/{name}/page.json")]
    public async Task<IActionResult> GetRegistrationPageAsync(string name)
    {
        var packages = await _packageService.FindAllByNameAsync(name);

        if (packages.Count == 0)
            return NotFound();

        return Ok(GetRegistrationPage(packages));
    }

    [HttpGet("v3/registration/{name}/{version}/leaf.json")]
    public async Task<IActionResult> GetRegistrationLeafAsync(string name, string version)
    {
        var package = await _packageService.TryFindByNameVersionAsync(name, version);

        if (package is null)
            return NotFound();

        return Ok(GetRegistrationLeaf(package));
    }

    [HttpGet("v3/registration/{name}/{version}/catalog-entry.json")]
    public async Task<IActionResult> GetCatalogEntryAsync(string name, string version)
    {
        var package = await _packageService.TryFindByNameVersionAsync(name, version);

        if (package is null)
            return NotFound();

        return Ok(GetCatalogEntry(package));
    }

    private RegistrationPageResponse GetRegistrationPage(IReadOnlyCollection<Package> packages)
    {
        var id = packages.First().Name.ToLowerInvariant();
        var leafs = packages.Select(GetRegistrationLeaf).ToArray();
        var lower = packages.Min(e => e.Version)!;
        var upper = packages.Max(e => e.Version)!;

        return new RegistrationPageResponse(
            _urlTool.AbsoluteUrl($"v3/registration/{id}/page.json"),
            leafs,
            lower,
            upper
        );
    }

    private RegistrationLeafResponse GetRegistrationLeaf(Package package)
    {
        var id = package.Name.ToLowerInvariant();
        var version = package.Version;

        return new RegistrationLeafResponse(
            _urlTool.AbsoluteUrl($"v3/registration/{id}/{version}/leaf.json"),
            GetCatalogEntry(package),
            _urlTool.AbsoluteUrl($"v3/package/{id}/{version}/{id}.{version}.nupkg")
        );
    }

    private CatalogEntryResponse GetCatalogEntry(Package package)
    {
        var id = package.Name.ToLowerInvariant();
        var name = package.Name;
        var version = package.Version;

        return new CatalogEntryResponse(
            _urlTool.AbsoluteUrl($"v3/registration/{id}/{version}/catalog-entry.json"),
            name,
            version
        );
    }
}
