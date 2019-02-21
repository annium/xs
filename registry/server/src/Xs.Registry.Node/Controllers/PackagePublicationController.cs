using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Node;
using Xs.Registry.Db.Shared;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Node.Controllers
{
    public class PackagePublicationController : ServerController<User>
    {
        private readonly Func<Instant> getInstant;

        private readonly IPackageService<Package, PackageDependency, PackagePayload> packageService;

        public PackagePublicationController(
            Func<Instant> getInstant,
            IPackageService<Package, PackageDependency, PackagePayload> packageService
        )
        {
            this.getInstant = getInstant;
            this.packageService = packageService;
        }

        [HttpPut("{package}")]
        [AuthorizeApi]
        public async Task<IActionResult> PublishPackageAsync([FromBody] PackagePayload payload)
        {
            if (payload == null)
                return BadRequest("Empty data");

            if (!ModelState.IsValid)
                return BadRequest("Incorrect data");

            payload.Published = getInstant();

            var result = await packageService.PublishPackageAsync(GetUser(), payload);
            switch (result)
            {
                case Abstract.Packages.ForbiddenResult res:
                    return Forbidden(res.Error);
                case Abstract.Packages.ConflictResult res:
                    return Conflict(res.Error);
                default:
                    return NoContent();
            }
        }
    }
}