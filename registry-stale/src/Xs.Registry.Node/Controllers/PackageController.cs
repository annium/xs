using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Core.Models;
using Xs.Execution;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Db;
using Xs.Registry.Core.Tools;
using Xs.Registry.Node.Models;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Storage;
using Xs.Registry.Node.Views;

namespace Xs.Registry.Node.Controllers
{
    public class PackageController : ServerController
    {
        private readonly Func<Instant> getInstant;

        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package> packageRepository;

        private readonly IPackageStorage packageStorage;

        private readonly IUrlHelper url;

        public PackageController(
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
        [Authorize(Access.Api)]
        public async Task<IActionResult> PublishPackageAsync([FromBody] PackagePayload packagePayload)
        {
            if (packagePayload == null)
                return BadRequest("Empty data");

            if (!ModelState.IsValid)
                return BadRequest("Incorrect data");

            packagePayload.Published = getInstant();
            var package = (Package) packagePayload;
            var packageStream = packagePayload.GetAttachment();
            var name = PackageName.Parse(package.Name);
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
                () => packageStorage.SaveAsync(name, version, packageStream),
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

            return Created(new { Ok = "Done", Success = true });
        }

        private async Task<IActionResult> UnpublishPackageAsync(PackageName name)
        {
            var allExisting = await packageRepository.FindAllByNameAsync(name);
            var version = allExisting.FirstOrDefault()?.Version;

            if (version == null)
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
            executor.With(() => packageRepository.DeleteByNameVersionAsync((string) name, (string) version));

            // if it was last package - delete metaPackage
            if (allExisting.Length == 1)
                executor.With(() => metaPackageRepository.DeleteByIdAsync(metaPackageId));

            await executor.RunAsync();

            return NoContent();
        }

        [HttpGet("{name}")]
        [Authorize(Access.Api)]
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

            var user = GetUser();

            // try load metaPackage; if exists - check permissions
            var metaPackage = await metaPackageRepository.GetByIdAsync(packages[0].MetaPackageId);
            if (!metaPackageManager.CheckPermission(user, metaPackage, Permission.Read))
                return Forbidden("You need read permission to get this package.");

            return Ok(new PackageView(packages, url));
        }

        [HttpGet("{name}/{version}.tgz")]
        [Authorize(Access.Api)]
        public async Task<IActionResult> DownloadPackageAsync([FromRoute] string name, [FromRoute] string version)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));

            var package = await packageRepository.FindByNameVersionAsync(name, version);
            if (package == null)
                return NotFound();

            var user = GetUser();

            // try load metaPackage; if exists - check permissions
            var metaPackage = await metaPackageRepository.GetByIdAsync(package.MetaPackageId);
            if (!metaPackageManager.CheckPermission(user, metaPackage, Permission.Read))
                return Forbidden("You need read permission to get this package.");

            if (!(await packageStorage.ExistsAsync(packageName, version)))
                return ServerError("Package file missing");

            package.Downloads++;
            await packageRepository.SaveAsync(package);

            var content = await packageStorage.GetAsync(packageName, version);

            return File(content, MediaTypeNames.Application.Octet);
        }
    }
}