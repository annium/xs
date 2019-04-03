using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        private readonly ILogger<PackagePublicationController> logger;

        public PackagePublicationController(
            Func<Instant> getInstant,
            IPackageService<Package, PackageDependency, PackagePayload> packageService,
            ILogger<PackagePublicationController> logger
        )
        {
            this.getInstant = getInstant;
            this.packageService = packageService;
            this.logger = logger;
        }

        [HttpPut("{package}")]
        [AuthorizeApi]
        public async Task<IActionResult> PublishPackageAsync(string package, [FromBody] PackagePayload payload)
        {
            if (payload == null)
            {
                logger.LogInformation($"Publication of {package} declined: Empty payload");
                return BadRequest("Empty data");
            }

            if (!ModelState.IsValid)
            {
                logger.LogInformation($"Publication of {package} declined: {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest("Incorrect data");
            }

            payload.Published = getInstant();

            var result = await packageService.PublishPackageAsync(GetUser(), payload);
            switch (result.Status)
            {
                case PackageStatus.Forbidden:
                    return Forbidden(result);
                case PackageStatus.Conflict:
                    return Conflict(result);
                default:
                    return NoContent();
            }
        }
    }
}