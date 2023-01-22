using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Server.Domain.Enums;
using Server.Domain.Models;
using Server.Main.Services;
using Server.Main.Views;
using Server.Shared.Auth;
using Server.Shared.Controllers;
using Server.Shared.Tools;

namespace Server.Main.Controllers;

[Route("packages")]
public class MetaPackagesController : ServerController<User>
{
    private readonly IMetaPackageService _metaPackageService;
    private readonly IMetaPackageManager _metaPackageManager;

    public MetaPackagesController(
        IMetaPackageService metaPackageService,
        IMetaPackageManager metaPackageManager
    )
    {
        _metaPackageService = metaPackageService;
        _metaPackageManager = metaPackageManager;
    }

    [HttpGet("search")]
    [Authorize(Access.Api | Access.Session)]
    public async Task<IActionResult> FindPackagesAsync(
        Guid ownerId = default(Guid),
        string type = null,
        string query = null,
        int page = 1,
        int count = 50
    )
    {
        var projectType = type is null ? null : ProjectType.Get(type);
        query = HttpUtility.UrlDecode(query);
        if (page < 1)
            return BadRequest("Page must be positive integer");
        if (count < 1)
            return BadRequest("Count must be positive integer");

        var packages = await _metaPackageService.FindAllAsync(GetUser().Id, ownerId, projectType, query, page, count);

        return Ok(packages.Select(p => new MetaPackageView(p)).ToArray());
    }

    [HttpGet("{type}/{name}")]
    [AuthorizeSession]
    public async Task<IActionResult> GetPackageAsync(string type, string name)
    {
        name = HttpUtility.UrlDecode(name);
        var package = await _metaPackageService.TryFindByTypeNameAsync(ProjectType.Get(type), name);

        if (package is null)
            return NotFound();

        var access = _metaPackageManager.GetAccess(package).ForUser(GetUser());
        if (!access.Has(Permission.Read))
            return new ObjectResult("You need read permission to get this package.") { StatusCode = (int) HttpStatusCode.Forbidden };

        return Ok(new MetaPackageView(package));
    }

    [HttpPost("{type}/{name}/permissions")]
    [AuthorizeSession]
    public async Task<IActionResult> UpdatePackagePermissionsAsync(string type, string name, [FromBody] MetaPackagePermission[] permissions)
    {
        name = HttpUtility.UrlDecode(name);
        var package = await _metaPackageService.TryFindByTypeNameAsync(ProjectType.Get(type), name);

        if (package is null)
            return NotFound();

        var access = _metaPackageManager.GetAccess(package).ForUser(GetUser());
        if (!access.IsOwner)
            return new ObjectResult("You need to be owner to update package permissions.") { StatusCode = (int) HttpStatusCode.Forbidden };

        await _metaPackageService.UpdatePermissionsAsync(package.Id, permissions);

        return NoContent();
    }
}