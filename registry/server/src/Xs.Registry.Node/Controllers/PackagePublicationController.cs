using System;
using System.Net;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Time;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        private readonly ITimeProvider _timeProvider;
        private readonly IPackageService<Package, PackageDependency, PackagePayload> _packageService;
        private readonly ILogger<PackagePublicationController> _logger;

        public PackagePublicationController(
            ITimeProvider timeProvider,
            IPackageService<Package, PackageDependency, PackagePayload> packageService,
            ILogger<PackagePublicationController> logger,
            IMediator mediator,
            IServiceProvider sp
        ) : base(mediator, sp)
        {
            _timeProvider = timeProvider;
            _packageService = packageService;
            _logger = logger;
        }

        [HttpPut("{package}")]
        [AuthorizeApi]
        public async Task<IActionResult> PublishPackageAsync(string package, [FromBody] PackagePayload payload)
        {
            if (payload == null)
            {
                _logger.LogInformation($"Publication of {package} declined: Empty payload");
                return BadRequest("Empty data");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation($"Publication of {package} declined: {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest("Incorrect data");
            }

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
}