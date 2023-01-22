using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Packages;
using Server.Domain.Models;
using Server.Dotnet.Models;
using Server.Dotnet.Payloads;
using Server.Dotnet.Views;
using Server.Shared.Controllers;
using Server.Shared.Extensions;

namespace Server.Dotnet.Controllers;

public class PackageRegistrationController : ServerController<User>
{
    private readonly IPackageService<Package, PackageDependency, PackagePayload> _packageService;
    private readonly IUrlHelper _url;

    public PackageRegistrationController(
        IPackageService<Package, PackageDependency, PackagePayload> packageService,
        IUrlHelper url
    )
    {
        _packageService = packageService;
        _url = url;
    }

    [HttpGet("v3/registration/{name}/index.json")]
    public async Task<IActionResult> GetRegistrationIndexAsync(string name, CancellationToken ct)
    {
        var packages = await _packageService.FindAllByNameAsync(name);

        if (packages.Count == 0)
            return NotFound();

        return Ok(new RegistrationIndexView(new[] { GetRegistrationPage(packages) }));
    }

    [HttpGet("v3/registration/{name}/page.json")]
    public async Task<IActionResult> GetRegistrationPageAsync(string name, CancellationToken ct)
    {
        var packages = await _packageService.FindAllByNameAsync(name);

        if (packages.Count == 0)
            return NotFound();

        return Ok(GetRegistrationPage(packages));
    }

    [HttpGet("v3/registration/{name}/{version}/leaf.json")]
    public async Task<IActionResult> GetRegistrationLeafAsync(string name, string version, CancellationToken ct)
    {
        var package = await _packageService.TryFindByNameVersionAsync(name, version);

        if (package is null)
            return NotFound();

        return Ok(GetRegistrationLeaf(package));
    }

    [HttpGet("v3/registration/{name}/{version}/catalog-entry.json")]
    public async Task<IActionResult> GetCatalogEntryAsync(string name, string version, CancellationToken ct)
    {
        var package = await _packageService.TryFindByNameVersionAsync(name, version);

        if (package is null)
            return NotFound();

        return Ok(GetCatalogEntry(package));
    }

    private RegistrationPageView GetRegistrationPage(IReadOnlyCollection<Package> packages)
    {
        var id = packages.First().Name.ToLowerInvariant();
        var leafs = packages.Select(GetRegistrationLeaf).ToArray();
        var lower = packages.Min(e => e.Version);
        var upper = packages.Max(e => e.Version);

        return new RegistrationPageView(_url.AbsoluteUri($"v3/registration/{id}/page.json"), leafs, lower, upper);
    }

    private RegistrationLeafView GetRegistrationLeaf(Package package)
    {
        var id = package.Name.ToLowerInvariant();
        var version = package.Version;

        return new RegistrationLeafView(
            _url.AbsoluteUri($"v3/registration/{id}/{version}/leaf.json"),
            GetCatalogEntry(package),
            _url.AbsoluteUri($"v3/package/{id}/{version}/{id}.{version}.nupkg")
        );
    }

    private CatalogEntryView GetCatalogEntry(Package package)
    {
        var id = package.Name.ToLowerInvariant();
        var name = package.Name;
        var version = package.Version;

        return new CatalogEntryView(_url.AbsoluteUri($"v3/registration/{id}/{version}/catalog-entry.json"), name, version);
    }
}