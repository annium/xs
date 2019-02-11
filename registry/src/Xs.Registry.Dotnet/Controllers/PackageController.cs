using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Execution;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Storage;
using Xs.Registry.Dotnet.Views;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet.Controllers
{
    [Route("packages")]
    public class PackageController : ServerController<User>
    {
        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package> packageRepository;
        
        private readonly IPackageStorage packageStorage;

        public PackageController(
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package> packageRepository,
            IPackageStorage packageStorage
        )
        {
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
        }

        [HttpGet("{name}")]
        [Authorize]
        public async Task<IActionResult> GetLatestPackageAsync(string name)
        {
            name = HttpUtility.UrlDecode(name);
            var package = await packageRepository.FindLatestByNameAsync(name);

            if (package == null)
                return NotFound();

            var access = (await metaPackageRepository.GetAccessByIdAsync(package.MetaPackageId)).ForUser(GetUser());
            if (!access.Has(Permission.Read))
                return Forbidden("You need read permission to get this package.");

            return Ok(new PackageView(package));
        }

        [HttpGet("{name}/{version}")]
        [Authorize]
        public async Task<IActionResult> GetPackageAsync(string name, string version)
        {
            name = HttpUtility.UrlDecode(name);
            var package = await packageRepository.FindByNameVersionAsync(name, version);

            if (package == null)
                return NotFound();

            var access = (await metaPackageRepository.GetAccessByIdAsync(package.MetaPackageId)).ForUser(GetUser());
            if (!access.Has(Permission.Read))
                return Forbidden("You need read permission to get this package.");

            return Ok(new PackageView(package));
        }

        [HttpDelete("{name}/{version}")]
        [Authorize]
        public async Task<IActionResult> DeletePackageAsync(string name, string version)
        {
            name = HttpUtility.UrlDecode(name);
            var versions = await packageRepository.FindAllByNameAsync(name);
            if (!versions.Any(p => p.Version == version))
                return NotFound();

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
    }
}