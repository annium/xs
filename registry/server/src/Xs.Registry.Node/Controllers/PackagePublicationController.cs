using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Xs.Execution;
using Xs.Registry.Db.Node;
using Xs.Registry.Db.Shared;
using Xs.Registry.Node.Models;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Storage;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Node.Controllers
{
    public class PackagePublicationController : ServerController<User>
    {
        private readonly Func<Instant> getInstant;

        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package, PackageDependency> packageRepository;

        private readonly IPackageStorage packageStorage;

        public PackagePublicationController(
            Func<Instant> getInstant,
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package, PackageDependency> packageRepository,
            IPackageStorage packageStorage
        )
        {
            this.getInstant = getInstant;
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
        }

        [HttpPut("{package}")]
        [AuthorizeApi]
        public async Task<IActionResult> PublishPackageAsync([FromBody] PackagePayload packagePayload)
        {
            {
                if (packagePayload == null)
                    return BadRequest("Empty data");

                if (!ModelState.IsValid)
                    return BadRequest("Incorrect data");

                var executor = Executor.Staged();
                var user = GetUser();

                packagePayload.Published = getInstant();

                var name = PackageName.Parse(packagePayload.Name);
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
                        await packageStorage.DeleteAsync(payload.PackageName, payload.Version);
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

                var version = payload.Versions[payload.Version];

                var pkg = new Package(
                    metaPackage.Id,
                    payload.Name,
                    payload.Version,
                    payload.Description,
                    payload.Published,
                    version.Main,
                    version.Distribution.Shasum,
                    version.Distribution.Integrity,
                    version.Dependencies.Select(d => new PackageDependency(DependencyType.Normal, d.Key, d.Value))
                    .Concat(version.DevDependencies.Select(d => new PackageDependency(DependencyType.Dev, d.Key, d.Value)))
                    .ToArray()
                );
                var packageName = PackageName.Parse(pkg.Name);

                executor.Stage(
                    () => packageStorage.SaveAsync(packageName, pkg.Version, payload.GetAttachment()),
                    () => packageStorage.DeleteAsync(packageName, pkg.Version)
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
        }
    }
}