using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Xs.Execution;
using Xs.Registry.Db.Node;
using Xs.Registry.Db.Shared;
using Xs.Registry.Node.Models;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Storage;
using Xs.Registry.Node.Views;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Node.Controllers
{
    public class PackagePublicationConsumptionController : ServerController<User>
    {
        private readonly Func<Instant> getInstant;

        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package> packageRepository;

        private readonly IPackageStorage packageStorage;

        private readonly IUrlHelper url;

        public PackagePublicationConsumptionController(
            Func<Instant> getInstant,
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package> packageRepository,
            IPackageStorage packageStorage,
            IUrlHelper url
        )
        {
            this.getInstant = getInstant;
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
            this.url = url;
        }

        [HttpPut("{package}")]
        [Authorize]
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

        private async Task<IActionResult> UnpublishPackageAsync(PackageName name)
        {
            // get available versions
            var versions = await packageRepository.FindAllByNameAsync(name);
            var version = versions.FirstOrDefault()?.Version;
            if (version == null)
                return NotFound();

            // load metaPackage and check permissions
            var metaPackage = await metaPackageRepository.GetByIdAsync(versions[0].MetaPackageId);
            var access = metaPackageManager.GetAccess(metaPackage).ForUser(GetUser());
            if (!access.Has(Permission.Unpublish))
                return Forbidden("You need unpublish permission to unpublish this package.");

            var executor = Executor.Batch();

            // delete from storage
            executor.With(() => packageStorage.DeleteAsync(name, version));

            // delete from db
            executor.With(() => packageRepository.DeleteByNameVersionAsync(name, version));

            // if it was last package - delete metaPackage
            if (versions.Length == 1)
                executor.With(() => metaPackageRepository.DeleteByIdAsync(metaPackage.Id));
            // else - update metaPackage
            else
            {
                // get latest version of all left except deleted (note - they are sorted from repository)
                var latest = versions.FirstOrDefault(p => p.Version != version);

                // if latest changed - need to update metaPackage
                if (latest.Version != metaPackage.Version)
                    executor.With(() => metaPackageRepository.UpdateInfoAsync(metaPackage.Id, latest));

                // and anyway - recount downloads
                executor.With(
                    async() => await metaPackageRepository.SetDownloadsAsync(
                        metaPackage.Id,
                        await packageRepository.CountAllDownloadsAsync(metaPackage.Name)
                    )
                );
            }

            await executor.RunAsync();

            return NoContent();
        }

        [HttpGet("{name}")]
        [Authorize]
        public Task<IActionResult> GetPackageAsync([FromRoute] string name, [FromQuery] bool write)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));

            return write ? UnpublishPackageAsync(packageName) : GetPackageAsync(packageName);
        }

        private async Task<IActionResult> GetPackageAsync(PackageName name)
        {
            var packages = await packageRepository.FindAllByNameAsync(name);
            if (packages.Length == 0)
                return NotFound();

            // try load metaPackage; if exists - check permissions
            var access = (await metaPackageRepository.GetAccessByIdAsync(packages[0].MetaPackageId)).ForUser(GetUser());
            if (!access.Has(Permission.Read))
                return Forbidden("You need read permission to get this package.");

            return Ok(new PackagesView(packages, url));
        }

        [HttpGet("{name}/{version}.tgz")]
        [Authorize]
        public async Task<IActionResult> DownloadPackageAsync([FromRoute] string name, [FromRoute] string version)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));

            var package = await packageRepository.FindByNameVersionAsync(packageName, version);
            if (package == null)
                return NotFound();

            var user = GetUser();

            // try load metaPackage; if exists - check permissions
            var access = (await metaPackageRepository.GetAccessByIdAsync(package.MetaPackageId)).ForUser(GetUser());
            if (!access.Has(Permission.Read))
                return Forbidden("You need read permission to get this package.");

            if (!(await packageStorage.ExistsAsync(packageName, version)))
                return ServerError("Package file missing");

            await packageRepository.IncrementDownloadsAsync(package.Id);
            await metaPackageRepository.IncrementDownloadsAsync(package.MetaPackageId);

            var content = await packageStorage.GetAsync(packageName, version);

            return File(content, MediaTypeNames.Application.Octet);
        }
    }
}