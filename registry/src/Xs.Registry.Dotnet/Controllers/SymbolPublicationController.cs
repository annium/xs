using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Db;
using Xs.Registry.Dotnet.Helpers;
using Xs.Registry.Dotnet.Models;
using Xs.Registry.Dotnet.Storage;

namespace Xs.Registry.Dotnet.Controllers
{
    public class SymbolPublicationController : ServerController
    {
        private static readonly HashSet<string> validExtensions = new HashSet<string>
        {
            ".pdb",
            ".nuspec",
            ".xml",
            ".psmdcp",
            ".rels",
            ".p7s"
        };

        private readonly IPackageRepository<Package> packageRepository;

        private readonly ISymbolStorage symbolStorage;

        public SymbolPublicationController(
            IPackageRepository<Package> packageRepository,
            ISymbolStorage symbolStorage
        )
        {
            this.packageRepository = packageRepository;
            this.symbolStorage = symbolStorage;
        }

        [HttpPut("api/v2/symbol")]
        [Authorize(Access.Api)]
        public async Task<IActionResult> PublishSymbolsAsync(CancellationToken token)
        {
            using(var symbolsStream = await Request.GetUploadStreamOrNullAsync(token))
            {
                if (symbolsStream == null)
                    return BadRequest("Use multipart/form-data to upload symbols.");

                using(var packageReader = new NuGet.Packaging.PackageArchiveReader(symbolsStream, leaveStreamOpen : true))
                {
                    await packageReader.ValidatePackageEntriesAsync(token);

                    var files = await GetPdbPathsOrNull(packageReader, token);
                    if (files == null)
                        return BadRequest("Ensure symbol package is valid.");

                    var name = packageReader.NuspecReader.GetId();
                    var version = packageReader.NuspecReader.GetVersion().ToNormalizedString();

                    // TODO: when applicable, add permissions usage

                    if ((await packageRepository.FindByNameVersionAsync(name, version)) == null)
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
            NuGet.Packaging.PackageArchiveReader reader,
            CancellationToken token
        )
        {
            var files = (await reader.GetFilesAsync(token)).ToList();

            return files.All(isValidFile) ?
                files.Where(e => Path.GetExtension(e) == ".pdb").ToList() :
                null;

            bool isValidFile(string path) =>
                !string.IsNullOrEmpty(Path.GetFileName(path)) &&
                !string.IsNullOrEmpty(Path.GetExtension(path)) &&
                validExtensions.Contains(Path.GetExtension(path));
        }
    }
}