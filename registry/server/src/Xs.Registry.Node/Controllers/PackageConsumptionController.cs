using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Node;
using Xs.Registry.Db.Shared;
using Xs.Registry.Node.Models;
using Xs.Registry.Node.Storage;
using Xs.Registry.Node.Views;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Node.Controllers
{
    public class PackageConsumptionController : ServerController<User>
    {
        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package> packageRepository;

        private readonly IPackageStorage packageStorage;

        private readonly IUrlHelper url;

        public PackageConsumptionController(
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package> packageRepository,
            IPackageStorage packageStorage,
            IUrlHelper url
        )
        {
            this.metaPackageRepository = metaPackageRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
            this.url = url;
        }

        [HttpGet("{name}")]
        [AuthorizeApi]
        public async Task<IActionResult> GetPackageAsync([FromRoute] string name)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));
            var packages = await packageRepository.FindAllByNameAsync(packageName);
            if (packages.Length == 0)
                return NotFound();

            // try load metaPackage; if exists - check permissions
            var access = (await metaPackageRepository.GetAccessByIdAsync(packages[0].MetaPackageId)).ForUser(GetUser());
            if (!access.Has(Permission.Read))
                return Forbidden("You need read permission to get this package.");

            return Ok(new PackagesView(packages, url));
        }

        [HttpGet("{name}/{version}.tgz")]
        [AuthorizeApi]
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
            var total = await packageRepository.CountAllDownloadsAsync(package.Name);
            await metaPackageRepository.SetDownloadsAsync(package.MetaPackageId, total);

            var content = await packageStorage.GetAsync(packageName, version);

            return File(content, MediaTypeNames.Application.Octet);
        }
    }
}