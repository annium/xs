using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using Server.Abstractions.Domain;
using Server.Abstractions.Services;
using Server.Dotnet.Domain;
using Server.Dotnet.Internal;
using Server.Dotnet.Internal.Extensions;
using Server.Dotnet.Views.Requests;
using Server.Shared.Auth;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;

namespace Server.Dotnet.Controllers;

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

    [HttpPut("api/v2/package")]
    [Authorize]
    public async Task<IActionResult> PublishPackageAsync()
    {
        await using var packageStream = await Request.GetUploadStreamOrNullAsync(CancellationToken.None);

        if (packageStream is null)
            return BadRequest("Use multipart/form-data to upload package.");

        var request = await ReadPackageFromStream(packageStream);

        var result = await _packageService.PublishPackageAsync(GetUser(), request);
        switch (result.Status)
        {
            case PackageStatus.Forbidden:
                return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Forbidden };
            case PackageStatus.Conflict:
                return Conflict(result);
            default:
                return NoContent();
        }
    }

    private async Task<PackageRequest> ReadPackageFromStream(Stream packageStream)
    {
        using var packageReader = new PackageArchiveReader(packageStream, leaveStreamOpen: true);

        await packageReader.ValidatePackageEntriesAsync(CancellationToken.None);

        var packageId = Guid.NewGuid();
        var nuspec = packageReader.NuspecReader;
        var dependencies = nuspec.GetDependencyGroups()
            .SelectMany(dependencyGroup =>
            {
                var framework = dependencyGroup.TargetFramework.GetShortFolderName()!;

                return dependencyGroup.Packages
                    .Select(dependency => new PackageDependency(packageId, framework, dependency.Id, dependency.VersionRange.ToNormalizedString()));
            })
            .ToArray();

        return new PackageRequest(
            packageId,
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