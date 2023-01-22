using System.Net;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Mvc;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Domain.Models;
using Server.Node.Models;
using Server.Node.Payloads;
using Server.Shared.Auth.Attributes;
using Server.Shared.Controllers;

namespace Server.Node.Controllers;

public class PackagePublicationController : ServerController<User>
{
    private readonly ITimeProvider _timeProvider;
    private readonly IPackageService<Package, PackageDependency, PackagePayload> _packageService;

    public PackagePublicationController(
        ITimeProvider timeProvider,
        IPackageService<Package, PackageDependency, PackagePayload> packageService
    )
    {
        _timeProvider = timeProvider;
        _packageService = packageService;
    }

    [HttpPut("{package}")]
    [AuthorizeApi]
    public async Task<IActionResult> PublishPackageAsync(string package, [FromBody] PackagePayload? payload)
    {
        if (payload is null)
            return BadRequest("Empty data");

        if (!ModelState.IsValid)
            return BadRequest("Incorrect data");

        payload.Published = _timeProvider.Now;

        var result = await _packageService.PublishPackageAsync(GetUser(), payload);
        switch (result.Status)
        {
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
            case PackageStatus.Conflict:
                return Conflict(result);
            default:
                return NoContent();
        }
    }
}