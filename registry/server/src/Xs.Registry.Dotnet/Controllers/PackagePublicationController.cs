using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Xs.Execution;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Helpers;
using Xs.Registry.Dotnet.Payloads;
using Xs.Registry.Dotnet.Storage;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackagePublicationController : ServerController<User>
    {
        private readonly Func<Instant> getInstant;

        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package> packageRepository;

        private readonly IPackageStorage packageStorage;

        public PackagePublicationController(
            Func<Instant> getInstant,
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package> packageRepository,
            IPackageStorage packageStorage
        )
        {
            this.getInstant = getInstant;
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
        }

        [HttpPut("api/v2/package")]
        [AuthorizeApi]
        public async Task<IActionResult> PublishPackageAsync()
        {
            using(var packageStream = await Request.GetUploadStreamOrNullAsync(CancellationToken.None))
            {
                if (packageStream == null)
                    return BadRequest("Use multipart/form-data to upload package.");

                var executor = Executor.Staged();
                var user = GetUser();

                var packagePayload = await readPackage(packageStream);

                var name = packagePayload.Name;
                var version = packagePayload.Version;

                // get metaPackage by (type, name)
                var metaPackage = await metaPackageRepository.FindByTypeNameAsync(Constants.ProjectType, name);

                var isNew = metaPackage == null;
                if (isNew)
                    metaPackage = await metaPackageRepository.CreateAsync(
                        metaPackageManager.Generate(user, Constants.ProjectType, packagePayload)
                    );

                var access = metaPackageManager.GetAccess(metaPackage).ForUser(user);

                // if new - publish new package
                if (isNew)
                    return await publishNewPackage(executor, metaPackage, access, packagePayload);

                // check version presence
                var republished = await packageRepository.FindByNameVersionAsync(name, version);

                // if present - republish package version, else - publish new package version
                return republished == null ?
                    await publishPackageVersion(executor, metaPackage, access, packagePayload) :
                    await republishPackageVersion(executor, metaPackage, access, packagePayload);
            }

            async Task<IActionResult> publishNewPackage(
                StageExecutor executor,
                MetaPackage metaPackage,
                UserMetaPackageAccess access,
                PackagePayload payload
            )
            {
                // commit stage is missing, cause manually called earlier; so just deletion stage
                executor.Stage(
                    () => { },
                    () => metaPackageRepository.DeleteByIdAsync(metaPackage.Id)
                );

                return await publishPackageVersion(executor, metaPackage, access, payload);
            }

            async Task<IActionResult> republishPackageVersion(
                StageExecutor executor,
                MetaPackage metaPackage,
                UserMetaPackageAccess access,
                PackagePayload payload
            )
            {
                if (!access.Has(Permission.Unpublish))
                    return Conflict($"Package {payload.Name} {payload.Version} already exists. You need republish permission to overwrite it.");

                executor.Stage(
                    async() =>
                    {
                        await packageStorage.DeleteAsync(payload.Name, payload.Version);
                        await packageRepository.DeleteByNameVersionAsync(payload.Name, payload.Version);
                    },
                    () => { }
                );

                return await publishPackageVersion(executor, metaPackage, access, payload);
            }

            async Task<IActionResult> publishPackageVersion(
                StageExecutor executor,
                MetaPackage metaPackage,
                UserMetaPackageAccess access,
                PackagePayload payload
            )
            {
                if (!access.Has(Permission.Publish))
                    return Forbidden($"You need publish permission to publish package {payload.Name} {payload.Version}.");

                var pkg = new Package(
                    metaPackage.Id,
                    payload.Name,
                    payload.Version,
                    payload.Description,
                    payload.Published,
                    payload.Dependencies
                );

                executor.Stage(
                    () => packageStorage.SaveAsync(pkg.Name, pkg.Version, payload.PackageStream, payload.NuspecStream),
                    () => packageStorage.DeleteAsync(pkg.Name, pkg.Version)
                );

                executor.Stage(
                    () => packageRepository.CreateAsync(pkg),
                    () => packageRepository.DeleteByNameVersionAsync(pkg.Name, pkg.Version)
                );

                executor.Stage(
                    async() => await metaPackageRepository.SetDownloadsAsync(
                        metaPackage.Id,
                        await packageRepository.CountAllDownloadsAsync(pkg.Name)
                    ),
                    async() => await metaPackageRepository.SetDownloadsAsync(
                        metaPackage.Id,
                        await packageRepository.CountAllDownloadsAsync(pkg.Name)
                    )
                );

                if (pkg.Version.CompareTo(metaPackage.Version) > 0)
                    executor.Stage(
                        () => metaPackageRepository.UpdateInfoAsync(metaPackage.Id, payload),
                        () => metaPackageRepository.UpdateInfoAsync(metaPackage.Id, metaPackage)
                    );

                await executor.RunAsync();

                return NoContent();
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