using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Core.Models;
using Xs.Registry.Core.Repositories;
using Xs.Registry.Core.Tools;
using Xs.Registry.Dotnet.Helpers;
using Xs.Registry.Dotnet.Models;
using Xs.Registry.Dotnet.Repositories;
using Xs.Registry.Dotnet.Storage;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackagePublicationController : ServerController
    {
        private readonly IMetadataManager metadataManager;

        private readonly IMetadataRepository metadataRepository;

        private readonly IPackageRepository packageRepository;

        private readonly IPackageStorage packageStorage;

        public PackagePublicationController(
            IMetadataManager metadataManager,
            IMetadataRepository metadataRepository,
            IPackageRepository packageRepository,
            IPackageStorage packageStorage
        )
        {
            this.metadataManager = metadataManager;
            this.metadataRepository = metadataRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
        }

        [HttpPut("api/v2/package")]
        [Authorize]
        public async Task<IActionResult> PublishPackageAsync(CancellationToken token)
        {
            using(var packageStream = await Request.GetUploadStreamOrNullAsync(token))
            {
                if (packageStream == null)
                    return BadRequest("Use multipart/form-data to upload package");

                using(var packageReader = new NuGet.Packaging.PackageArchiveReader(packageStream, leaveStreamOpen : true))
                {
                    await packageReader.ValidatePackageEntriesAsync(token);

                    var package = ReadPackage(packageReader.NuspecReader);
                    var name = package.Name;
                    var version = package.Version.ToNormalizedString();

                    var user = GetUser();

                    // try load metadata; if exists - check permissions
                    var metadata = await metadataRepository.FindByProjectTypePackageNameAsync(Constants.ProjectType, name);
                    if (metadata != null && !metadataManager.CheckPermission(user, metadata, Permission.Publish))
                        return Forbidden("You need publish permission to publish new package");

                    // if package exists - either can rewrite if permission granted, or it's conflict
                    var exists = (await packageRepository.FindByNameVersionAsync(name, version)) != null;
                    if (exists && !metadataManager.CheckPermission(user, metadata, Permission.Republish))
                        return Conflict($"Package {name} {version} already exists. You need republish permission to overwrite it");

                    // if exists - delete old
                    if (exists)
                    {
                        await packageStorage.DeleteAsync(name, version);
                        await packageRepository.DeleteByNameVersionAsync(name, version);
                    }

                    // persist to storage
                    await packageStorage.SaveAsync(
                        name,
                        version,
                        packageStream,
                        await packageReader.GetNuspecAsync(token)
                    );

                    // if no metadata - generate and save
                    if (metadata == null)
                        await metadataRepository.SaveAsync(metadataManager.Generate(user, Constants.ProjectType, name));

                    // persist to db
                    await packageRepository.SaveAsync(package);
                }

                return NoContent();
            }
        }

        [HttpDelete("api/v2/package/{name}/{version}")]
        [Authorize]
        public async Task<IActionResult> UnpublishPackageAsync(string name, string version, CancellationToken token)
        {
            var allExisting = await packageRepository.FindAllByNameAsync(name);
            var exists = allExisting.Any(e => e.Version.ToNormalizedString() == version);

            if (!exists)
                return NotFound();

            var user = GetUser();

            // load metadata and check permissions
            var metadata = await metadataRepository.FindByProjectTypePackageNameAsync(Constants.ProjectType, name);
            if (!metadataManager.CheckPermission(user, metadata, Permission.Unpublish))
                return Forbidden("You need unpublish permission to unpublish this package");

            // delete from storage
            await packageStorage.DeleteAsync(name, version);

            // if it was last package - delete metadata
            if (allExisting.Length == 1)
                await metadataRepository.DeleteByProjectTypePackageNameAsync(Constants.ProjectType, name);

            // delete from db
            await packageRepository.DeleteByNameVersionAsync(name, version);

            return NoContent();
        }

        private Package ReadPackage(NuGet.Packaging.NuspecReader reader)
        {
            var dependencyGroups = reader.GetDependencyGroups().ToDictionary(
                e => e.TargetFramework,
                e => e.Packages.Select(p => (p.Id, p.VersionRange)).ToArray().AsEnumerable()
            );

            return new Package(
                reader.GetId(),
                reader.GetVersion(),
                reader.GetDescription(),
                dependencyGroups
            );
        }
    }
}