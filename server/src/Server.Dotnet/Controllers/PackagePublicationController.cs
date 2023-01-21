using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using Server.Abstractions.Packages;
using Server.Db.Dotnet.Models;
using Server.Db.Shared.Models;
using Server.Dotnet.Helpers;
using Server.Dotnet.Payloads;
using Server.Shared.Auth;
using Server.Shared.Helpers;

namespace Server.Dotnet.Controllers;

public class PackagePublicationController : ServerController<User>
{
    private readonly ITimeProvider _timeProvider;
    private readonly IPackageService<Package, PackageDependency, PackagePayload> _packageService;

    public PackagePublicationController(
        ITimeProvider timeProvider,
        IPackageService<Package, PackageDependency, PackagePayload> packageService,
        IMediator mediator,
        IServiceProvider sp
    ) : base(mediator, sp)
    {
        _timeProvider = timeProvider;
        _packageService = packageService;
    }

    [HttpPut("api/v2/package")]
    [AuthorizeApi]
    public async Task<IActionResult> PublishPackageAsync()
    {
        await using (var packageStream = await Request.GetUploadStreamOrNullAsync(CancellationToken.None))
        {
            if (packageStream is null)
                return BadRequest("Use multipart/form-data to upload package.");

            var payload = await ReadPackage(packageStream);

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

        async Task<PackagePayload> ReadPackage(Stream packageStream)
        {
            using (var packageReader = new PackageArchiveReader(packageStream, leaveStreamOpen: true))
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
                    _timeProvider.Now,
                    dependencies,
                    packageStream,
                    packageReader.GetNuspec()
                );
            }
        }
    }
}