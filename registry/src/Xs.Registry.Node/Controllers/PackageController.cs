using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Core.Models;
using Xs.Execution;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Repositories;
using Xs.Registry.Core.Tools;
using Xs.Registry.Node.Models;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Repositories;
using Xs.Registry.Node.Storage;
using Xs.Registry.Node.Views;

namespace Xs.Registry.Node.Controllers
{
    public class PackageController : ServerController
    {
        private readonly IMetadataManager metadataManager;

        private readonly IMetadataRepository metadataRepository;

        private readonly IPackageRepository packageRepository;

        private readonly IPackageStorage packageStorage;

        private readonly IUrlHelper url;

        public PackageController(
            IMetadataManager metadataManager,
            IMetadataRepository metadataRepository,
            IPackageRepository packageRepository,
            IPackageStorage packageStorage,
            IUrlHelper url
        )
        {
            this.metadataManager = metadataManager;
            this.metadataRepository = metadataRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
            this.url = url;
        }

        [HttpPut("{package}")]
        [Authorize]
        public async Task<IActionResult> PublishPackageAsync([FromBody] PackagePayload packagePayload)
        {
            if (packagePayload == null)
                return BadRequest(new { common = new [] { "Empty data" } });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var package = (Package) packagePayload;
            var packageStream = packagePayload.GetAttachment();
            var name = package.Name;
            var version = package.Version;

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

            var executor = Exec.Staged();

            // persist to storage
            executor.Stage(
                () => packageStorage.SaveAsync(name, version, packageStream),
                () => packageStorage.DeleteAsync(name, version)
            );

            // if no metadata - generate and save
            if (metadata == null)
                executor.Stage(
                    () => metadataRepository.SaveAsync(metadataManager.Generate(user, Constants.ProjectType, name)),
                    () => metadataRepository.DeleteByProjectTypePackageNameAsync(Constants.ProjectType, name)
                );

            // persist to db
            executor.Stage(
                () => packageRepository.SaveAsync(package),
                () => packageRepository.DeleteByNameVersionAsync(name, version)
            );

            return Created(new { Ok = "Done", Success = true });
        }

        private async Task<IActionResult> UnpublishPackageAsync(PackageName name)
        {
            var allExisting = await packageRepository.FindAllByNameAsync((string) name);
            var version = allExisting.OrderByDescending(e => e.Version).FirstOrDefault()?.Version;

            if (version == null)
                return NotFound();

            var user = GetUser();

            // load metadata and check permissions
            var metadata = await metadataRepository.FindByProjectTypePackageNameAsync(Constants.ProjectType, (string) name);
            if (!metadataManager.CheckPermission(user, metadata, Permission.Unpublish))
                return Forbidden("You need unpublish permission to unpublish this package");

            // delete from storage
            await packageStorage.DeleteAsync(name, version);

            // if it was last package - delete metadata
            if (allExisting.Length == 1)
                await metadataRepository.DeleteByProjectTypePackageNameAsync(Constants.ProjectType, (string) name);

            // delete from db
            await packageRepository.DeleteByNameVersionAsync((string) name, (string) version);

            return NoContent();
        }

        [HttpGet("{name}")]
        [Authorize]
        public Task<IActionResult> GetPackageAsync([FromRoute] string name, [FromQuery] bool write)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));

            return write?UnpublishPackageAsync(packageName) : GetPackageAsync(packageName);
        }

        private async Task<IActionResult> GetPackageAsync(PackageName name)
        {
            var packages = await packageRepository.FindAllByNameAsync(name);
            if (packages.Length == 0)
                return NotFound();

            var user = GetUser();

            // try load metadata; if exists - check permissions
            var metadata = await metadataRepository.FindByProjectTypePackageNameAsync(Constants.ProjectType, name);
            if (metadata != null && !metadataManager.CheckPermission(user, metadata, Permission.Read))
                return Forbidden("You need read permission to get this package");

            return Ok(new PackageView(packages, url));
        }

        [HttpGet("{name}/{version}.tgz")]
        [Authorize]
        public async Task<IActionResult> DownloadPackageAsync([FromRoute] string name, [FromRoute] string version)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));

            if ((await packageRepository.FindByNameVersionAsync(packageName, version)) == null)
                return NotFound();

            var user = GetUser();

            // try load metadata; if exists - check permissions
            var metadata = await metadataRepository.FindByProjectTypePackageNameAsync(Constants.ProjectType, packageName);
            if (metadata != null && !metadataManager.CheckPermission(user, metadata, Permission.Read))
                return Forbidden("You need read permission to get this package");

            var content = await packageStorage.GetAsync(packageName, version);

            return File(content, MediaTypeNames.Application.Octet);
        }
    }
}