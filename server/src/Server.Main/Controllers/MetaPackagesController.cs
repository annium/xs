using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Server.Main.Internal;
using Server.Main.Services;
using Server.Main.Views.Responses;
using Server.Shared.Auth;
using Server.Shared.Controllers;
using Server.Shared.Domain.Enums;
using Server.Shared.Domain.Models;
using Server.Shared.Tools;

namespace Server.Main.Controllers;

[Area(Constants.Project)]
[Route("[area]/packages")]
public class MetaPackagesController : ServerController<User>
{
    private readonly IMetaPackageService _metaPackageService;
    private readonly IMetaPackageTool _metaPackageTool;

    public MetaPackagesController(
        IMetaPackageService metaPackageService,
        IMetaPackageTool metaPackageTool
    )
    {
        _metaPackageService = metaPackageService;
        _metaPackageTool = metaPackageTool;
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> FindPackagesAsync(
        string? type = null,
        string? query = null,
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

        var packages = await _metaPackageService.FindAllAsync(GetUser().Id, projectType, query, page, count);

        return Ok(packages.Select(p => new MetaPackageResponse(p)).ToArray());
    }

    [HttpGet("{type}/{name}")]
    [Authorize]
    public async Task<IActionResult> GetPackageAsync(string type, string name)
    {
        name = HttpUtility.UrlDecode(name);
        var package = await _metaPackageService.TryFindByTypeNameAsync(ProjectType.Get(type), name);

        if (package is null)
            return NotFound();

        var access = _metaPackageTool.GetAccess(package).ForUser(GetUser());
        return access.Has(Permission.Read)
            ? Ok(new MetaPackageResponse(package))
            : new ObjectResult("You need read permission to get this package.") { StatusCode = (int)HttpStatusCode.Forbidden };
    }

    [HttpPost("{type}/{name}/permissions")]
    [Authorize]
    public async Task<IActionResult> UpdatePackagePermissionsAsync(string type, string name, [FromBody] MetaPackagePermission[] permissions)
    {
        name = HttpUtility.UrlDecode(name);
        var package = await _metaPackageService.TryFindByTypeNameAsync(ProjectType.Get(type), name);

        if (package is null)
            return NotFound();

        var access = _metaPackageTool.GetAccess(package).ForUser(GetUser());
        if (!access.IsOwner)
            return new ObjectResult("You need to be owner to update package permissions.") { StatusCode = (int)HttpStatusCode.Forbidden };

        await _metaPackageService.UpdatePermissionsAsync(package.Id, permissions);

        return NoContent();
    }
}