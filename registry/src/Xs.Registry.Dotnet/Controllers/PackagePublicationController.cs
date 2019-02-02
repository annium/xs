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
using Xs.Registry.Core.Db;
using Xs.Registry.Core.Tools;
using Xs.Registry.Dotnet.Helpers;
using Xs.Registry.Dotnet.Models;
using Xs.Registry.Dotnet.Storage;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackagePublicationController : ServerController
    {
        private readonly Func<DateTime> getTime;

        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package> packageRepository;

        private readonly IPackageStorage packageStorage;

        public PackagePublicationController(
            Func<DateTime> getTime,
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package> packageRepository,
            IPackageStorage packageStorage
        )
        {
            this.getTime = getTime;
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
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

                    // try load metaPackage; if exists - check permissions
                    var metaPackage = latest == null ?
                        metaPackageManager.Generate(user) :
                        await metaPackageRepository.GetByIdAsync(latest.MetaPackageId);

                    // check publish permissions, if latest package found
                    if (latest != null && !metaPackageManager.CheckPermission(user, metaPackage, Permission.Publish))
                        return Forbidden($"You need publish permission to publish package {name} {version}.");

                    // check republish permission if current package found - otherwise it's conflict
                    if (current != null && !metaPackageManager.CheckPermission(user, metaPackage, Permission.Republish))
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

                    // if no latest - save new metaPackage
                    if (latest == null)
                        executor.Stage(
                            () => metaPackageRepository.SaveAsync(metaPackage),
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

            // load metaPackage and check permissions
            var metaPackageId = allExisting[0].MetaPackageId;
            var metaPackage = await metaPackageRepository.GetByIdAsync(metaPackageId);
            if (!metaPackageManager.CheckPermission(user, metaPackage, Permission.Unpublish))
                return Forbidden("You need unpublish permission to unpublish this package.");

            var executor = Executor.Batch();

            // delete from storage
            executor.With(() => packageStorage.DeleteAsync(name, version));

            // delete from db
            executor.With(() => packageRepository.DeleteByNameVersionAsync(name, version));

            // if it was last package - delete metaPackage
            if (allExisting.Length == 1)
                executor.With(() => metaPackageRepository.DeleteByIdAsync(metaPackageId));

            await executor.RunAsync();

            return NoContent();
        }
    }
}