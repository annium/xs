using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using Server.Abstractions.Services;
using Server.Domain.Models;
using Server.Dotnet.Domain;
using Server.Dotnet.Internal;
using Server.Dotnet.Internal.Extensions;
using Server.Dotnet.Views.Requests;
using Server.Shared.Auth.Attributes;
using Server.Shared.Controllers;

namespace Server.Dotnet.Controllers;

[Area(Constants.Project)]
[Route("[area]")]
public class SymbolPublicationController : ServerController<User>
{
    private static readonly HashSet<string> ValidExtensions = new()
    {
        ".pdb",
        ".nuspec",
        ".xml",
        ".psmdcp",
        ".rels",
        ".p7s"
    };

    private readonly IPackageService<Package, PackageDependency, PackageRequest> _packageService;

    public SymbolPublicationController(
        IPackageService<Package, PackageDependency, PackageRequest> packageService
    )
    {
        _packageService = packageService;
    }

    [HttpPut("api/v2/symbol")]
    [AuthorizeApi]
    public async Task<IActionResult> PublishSymbolsAsync(CancellationToken ct)
    {
        await using var symbolsStream = await Request.GetUploadStreamOrNullAsync(ct);

        if (symbolsStream is null)
            return BadRequest("Use multipart/form-data to upload symbols.");

        using var packageReader = new PackageArchiveReader(symbolsStream, leaveStreamOpen: true);
        await packageReader.ValidatePackageEntriesAsync(ct);

        var files = GetPdbPathsOrNull(await packageReader.GetFilesAsync(ct));
        if (files is null)
            return BadRequest("Ensure symbol package is valid.");

        var name = packageReader.NuspecReader.GetId();
        var version = packageReader.NuspecReader.GetVersion().ToNormalizedString();

        // TODO: when applicable, add permissions usage

        if (await _packageService.TryFindByNameVersionAsync(name, version) is null)
            return NotFound($"Package {name} {version} doesn't exist.");

        foreach (var file in files)
        {
            var _ = packageReader.GetStream(file);
            // TODO: write symbol's content to disk. Need consuming flow to understand how to do this
        }

        return NoContent();
    }

    private IReadOnlyCollection<string>? GetPdbPathsOrNull(IEnumerable<string> files)
    {
        var filesArray = files.ToArray();

        return filesArray.All(IsValidFile) ? filesArray.Where(e => Path.GetExtension(e) == ".pdb").ToArray() : null;

        static bool IsValidFile(string path) =>
            !string.IsNullOrEmpty(Path.GetFileName(path)) &&
            !string.IsNullOrEmpty(Path.GetExtension(path)) &&
            ValidExtensions.Contains(Path.GetExtension(path));
    }
}