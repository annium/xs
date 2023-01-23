using System.Net;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Domain.Models;
using Server.Node.Domain;
using Server.Node.Internal;
using Server.Node.Views.Requests;
using Server.Shared.Controllers;

namespace Server.Node.Controllers;

[Area(Constants.Project)]
[Route("[area]")]
public class PackagePublicationController : ServerController<User>
{
    private readonly ITimeProvider _timeProvider;
    private readonly IPackageService<Package, PackageDependency, PackageRequest> _packageService;

    public PackagePublicationController(
        ITimeProvider timeProvider,
        IPackageService<Package, PackageDependency, PackageRequest> packageService
    )
    {
        _timeProvider = timeProvider;
        _packageService = packageService;
    }

    [HttpPut("{package}")]
    [Authorize]
    public async Task<IActionResult> PublishPackageAsync(string package, [FromBody] PackageRequest? request)
    {
        if (request is null)
            return BadRequest("Empty data");

        if (!ModelState.IsValid)
            return BadRequest("Incorrect data");

        request.Published = _timeProvider.Now;

        var result = await _packageService.PublishPackageAsync(GetUser(), request);

        return result.Status switch
        {
            PackageStatus.Forbidden => new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden },
            PackageStatus.Conflict  => Conflict(result),
            _                       => NoContent()
        };
    }
}