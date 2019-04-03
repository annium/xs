using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Node;
using Xs.Registry.Db.Shared;
using Xs.Registry.Node.Models;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Views;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Node.Controllers
{
    public class PackageConsumptionController : ServerController<User>
    {
        private readonly IPackageService<Package, PackageDependency, PackagePayload> packageService;

        private readonly Storage.IPackageStorage packageStorage;

        private readonly IUrlHelper url;

        public PackageConsumptionController(
            IPackageService<Package, PackageDependency, PackagePayload> packageService,
            Storage.IPackageStorage packageStorage,
            IUrlHelper url
        )
        {
            this.packageService = packageService;
            this.packageStorage = packageStorage;
            this.url = url;
        }

        [HttpGet("{name}")]
        [AuthorizeApi]
        public async Task<IActionResult> GetPackageAsync([FromRoute] string name)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));
            var result = await packageService.GetPackagesAsync(GetUser(), packageName);
            switch (result.Status)
            {
                case PackageStatus.NotFound:
                    return NotFound();
                case PackageStatus.Forbidden:
                    return Forbidden(result);
                case PackageStatus.OK:
                    return Ok(new PackagesView(result.Data, url));
                default:
                    return NotFound();
            }
        }

        [HttpGet("{name}/{version}.tgz")]
        [AuthorizeApi]
        public async Task<IActionResult> DownloadPackageAsync([FromRoute] string name, [FromRoute] string version)
        {
            var packageName = PackageName.Parse(HttpUtility.UrlDecode(name));
            var result = await packageService.ProcessDownloadAsync(GetUser(), packageName, version, true);
            switch (result.Status)
            {
                case PackageStatus.NotFound:
                    return NotFound();
                case PackageStatus.Forbidden:
                    return Forbidden(result);
                case PackageStatus.InternalError:
                    return ServerError(result);
            }

            var content = await packageStorage.GetAsync(packageName, version);

            return File(content, MediaTypeNames.Application.Octet);
        }
    }
}