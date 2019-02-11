using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
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

        public PackageController(
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package> packageRepository
        )
        {
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
            this.packageRepository = packageRepository;
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
            var package = await packageRepository.FindByNameVersionAsync(name, version);

            if (package == null)
                return NotFound();

            var access = (await metaPackageRepository.GetAccessByIdAsync(package.MetaPackageId)).ForUser(GetUser());
            if (!access.Has(Permission.Unpublish))
                return Forbidden("You need unpublish permission to unpublish this package.");

            await packageRepository.DeleteByIdAsync(package.Id);

            return NoContent();
        }
    }
}