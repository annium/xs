using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Core.Helpers;
using Xs.Core.Models;
using Xs.Execution;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Repositories;
using Xs.Registry.Core.Tools;
using Xs.Registry.Dotnet.Helpers;
using Xs.Registry.Dotnet.Models;
using Xs.Registry.Dotnet.Storage;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackagePublicationController : ServerController
    {
        private readonly Func<DateTime> getTime;

        private readonly IMetadataManager metadataManager;

        private readonly IMetadataRepository metadataRepository;

        private readonly IPackageRepository<Package> packageRepository;

        private readonly IPackageStorage packageStorage;

        public PackagePublicationController(
            Func<DateTime> getTime,
            IMetadataManager metadataManager,
            IMetadataRepository metadataRepository,
            IPackageRepository<Package> packageRepository,
            IPackageStorage packageStorage
        )
        {
            this.getTime = getTime;
            this.metadataManager = metadataManager;
            this.metadataRepository = metadataRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
        }

        [HttpPut("api/v2/package")]
        [Authorize(Access.Api)]
        public async Task<IActionResult> PublishPackageAsync(CancellationToken token)
        {
            using(var packageStream = await Request.GetUploadStreamOrNullAsync(token))
            {
                if (packageStream == null)
                    return BadRequest("Use multipart/form-data to upload package.");

                using(var packageReader = new NuGet.Packaging.PackageArchiveReader(packageStream, leaveStreamOpen : true))
                {
                    await packageReader.ValidatePackageEntriesAsync(token);

                    var package = ReadPackage(packageReader.NuspecReader);
                    var name = package.Name;
                    var version = package.Version;

                    var user = GetUser();

                    // find existing and latest packages
                    var latest = await packageRepository.FindLatestByNameAsync(name);
                    var current = await packageRepository.FindByNameVersionAsync(name, version);

                    // try load metadata; if exists - check permissions
                    var metadata = latest == null ?
                        metadataManager.Generate(user) :
                        await metadataRepository.GetByIdAsync(latest.MetadataId);

                    // check publish permissions, if latest package found
                    if (latest != null && !metadataManager.CheckPermission(user, metadata, Permission.Publish))
                        return Forbidden($"You need publish permission to publish package {name} {version}.");

                    // check republish permission if current package found - otherwise it's conflict
                    if (current != null && !metadataManager.CheckPermission(user, metadata, Permission.Republish))
                        return Conflict($"Package {name} {version} already exists. You need republish permission to overwrite it.");

                    var executor = Executor.Staged();

                    // if current package exists - delete it
                    if (current != null)
                        executor.Stage(
                            async() =>
                            {
                                await packageStorage.DeleteAsync(name, version);
                                await packageRepository.DeleteByNameVersionAsync(name, version);
                            },
                            () => { }
                        );

                    // persist to storage
                    executor.Stage(
                        async() => await packageStorage.SaveAsync(
                            name,
                            version,
                            packageStream,
                            await packageReader.GetNuspecAsync(token)
                        ),
                        () => packageStorage.DeleteAsync(name, version)
                    );

                    // persist to db
                    executor.Stage(
                        () => packageRepository.SaveAsync(package),
                        () => packageRepository.DeleteByNameVersionAsync(name, version)
                    );

                    // if no latest - save new metadata
                    if (latest == null)
                        executor.Stage(
                            () => metadataRepository.SaveAsync(metadata),
                            () => { }
                        );

                    await executor.RunAsync();
                }

                return NoContent();
            }

            Package ReadPackage(NuGet.Packaging.NuspecReader reader)
            {
                var dependencyGroups = reader.GetDependencyGroups().ToDictionary(
                    e => e.TargetFramework,
                    e => e.Packages.ToDictionary(p => p.Id, p => p.VersionRange).ToReadOnly()
                );

                return new Package(
                    reader.GetId(),
                    reader.GetVersion(),
                    reader.GetDescription(),
                    dependencyGroups,
                    getTime(),
                    0
                );
            }
        }

        [HttpDelete("api/v2/package/{name}/{version}")]
        [Authorize(Access.Api)]
        public async Task<IActionResult> UnpublishPackageAsync(string name, string version, CancellationToken token)
        {
            var allExisting = await packageRepository.FindAllByNameAsync(name);
            var exists = allExisting.Any(e => e.Version == version);

            if (!exists)
                return NotFound();

            var user = GetUser();

            // load metadata and check permissions
            var metadataId = allExisting[0].MetadataId;
            var metadata = await metadataRepository.GetByIdAsync(metadataId);
            if (!metadataManager.CheckPermission(user, metadata, Permission.Unpublish))
                return Forbidden("You need unpublish permission to unpublish this package.");

            var executor = Executor.Batch();

            // delete from storage
            executor.With(() => packageStorage.DeleteAsync(name, version));

            // delete from db
            executor.With(() => packageRepository.DeleteByNameVersionAsync(name, version));

            // if it was last package - delete metadata
            if (allExisting.Length == 1)
                executor.With(() => metadataRepository.DeleteByIdAsync(metadataId));

            await executor.RunAsync();

            return NoContent();
        }
    }
}