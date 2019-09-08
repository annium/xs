using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Helpers;
using Xs.Registry.Dotnet.Payloads;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackagePublicationController : ServerController<User>
    {
        private readonly Func<Instant> getInstant;

        private readonly IPackageService<Package, PackageDependency, PackagePayload> packageService;

        public PackagePublicationController(
            Func<Instant> getInstant,
            IPackageService<Package, PackageDependency, PackagePayload> packageService,
            IMediator mediator
        ) : base(mediator)
        {
            this.getInstant = getInstant;
            this.packageService = packageService;
        }

        [HttpPut("api/v2/package")]
        [AuthorizeApi]
        public async Task<IActionResult> PublishPackageAsync()
        {
            using(var packageStream = await Request.GetUploadStreamOrNullAsync(CancellationToken.None))
            {
                if (packageStream == null)
                    return BadRequest("Use multipart/form-data to upload package.");

                var payload = await readPackage(packageStream);

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

            async Task<PackagePayload> readPackage(Stream packageStream)
            {
                using(var packageReader = new NuGet.Packaging.PackageArchiveReader(packageStream, leaveStreamOpen : true))
                {
                    await packageReader.ValidatePackageEntriesAsync(CancellationToken.None);

                    var nuspec = packageReader.NuspecReader;
                    var dependencies = nuspec.GetDependencyGroups()
                        .SelectMany(g =>
                        {
                            var framework = g.TargetFramework.GetShortFolderName();
                            return g.Packages.Select(d => new PackageDependency(framework, d.Id, d.VersionRange.ToNormalizedString()));
                        })
                        .ToArray();

                    return new PackagePayload(
                        nuspec.GetId(),
                        nuspec.GetVersion().ToNormalizedString(),
                        nuspec.GetDescription(),
                        getInstant(),
                        dependencies,
                        packageStream,
                        packageReader.GetNuspec()
                    );
                }
            }
        }
    }
}