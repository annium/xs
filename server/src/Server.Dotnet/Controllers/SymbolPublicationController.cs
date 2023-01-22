using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using Server.Db.Repositories;
using Server.Domain.Models;
using Server.Dotnet.Helpers;
using Server.Dotnet.Models;
using Server.Dotnet.Storage;
using Server.Shared.Auth;
using Server.Shared.Controllers;

namespace Server.Dotnet.Controllers;

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

    private readonly IPackageRepository<Package, PackageDependency> _packageRepository;

    private readonly ISymbolStorage _symbolStorage;

    public SymbolPublicationController(
        IPackageRepository<Package, PackageDependency> packageRepository,
        ISymbolStorage symbolStorage
    )
    {
        _packageRepository = packageRepository;
        _symbolStorage = symbolStorage;
    }

    [HttpPut("api/v2/symbol")]
    [AuthorizeApi]
    public async Task<IActionResult> PublishSymbolsAsync(CancellationToken ct)
    {
        await using (var symbolsStream = await Request.GetUploadStreamOrNullAsync(ct))
        {
            if (symbolsStream is null)
                return BadRequest("Use multipart/form-data to upload symbols.");

            using (var packageReader = new PackageArchiveReader(symbolsStream, leaveStreamOpen: true))
            {
                await packageReader.ValidatePackageEntriesAsync(ct);

                var files = await GetPdbPathsOrNull(packageReader, ct);
                if (files is null)
                    return BadRequest("Ensure symbol package is valid.");

                var name = packageReader.NuspecReader.GetId();
                var version = packageReader.NuspecReader.GetVersion().ToNormalizedString();

                // TODO: when applicable, add permissions usage

                if ((await _packageRepository.FindByNameVersionAsync(name, version)) is null)
                    return NotFound($"Package {name} {version} doesn't exist.");

                foreach (var file in files)
                {
                    var stream = packageReader.GetStream(file);
                    // TODO: write symbol's content to disk. Need consuming flow to understand how to do this
                }
            }

            return NoContent();
        }
    }

    private async Task<IReadOnlyList<string>> GetPdbPathsOrNull(
        PackageArchiveReader reader,
        CancellationToken ct
    )
    {
        var files = (await reader.GetFilesAsync(ct)).ToList();

        return files.All(IsValidFile) ? files.Where(e => Path.GetExtension(e) == ".pdb").ToList() : null;

        bool IsValidFile(string path) =>
            !string.IsNullOrEmpty(Path.GetFileName(path)) &&
            !string.IsNullOrEmpty(Path.GetExtension(path)) &&
            ValidExtensions.Contains(Path.GetExtension(path));
    }
}