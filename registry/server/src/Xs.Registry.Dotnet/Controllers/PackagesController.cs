using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Payloads;
using Xs.Registry.Dotnet.Views;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet.Controllers
{
    [Route("packages")]
    public class PackagesController : ServerController<User>
    {
        private readonly IPackageService<Package, PackageDependency, PackagePayload> packageService;

        public PackagesController(
            IPackageService<Package, PackageDependency, PackagePayload> packageService
        )
        {
            this.packageService = packageService;
        }

        [HttpGet("{name}")]
        [AuthorizeApi]
        public async Task<IActionResult> GetPackagesAsync(string name)
        {
            name = HttpUtility.UrlDecode(name);
            var result = await packageService.GetPackagesAsync(GetUser(), name);
            switch (result.Status)
            {
                case PackageStatus.NotFound:
                    return NotFound();
                case PackageStatus.Forbidden:
                    return Forbidden(result);
                case PackageStatus.OK:
                    return Ok(result.Data.Select(p => new PackageView(p)).ToArray());
                default:
                    return NotFound();
            }
        }

        [HttpDelete("{name}/{version}")]
        [AuthorizeApi]
        public async Task<IActionResult> DeletePackageAsync(string name, string version)
        {
            name = HttpUtility.UrlDecode(name);
            var result = await packageService.UnpublishPackageAsync(GetUser(), name, version);
            switch (result.Status)
            {
                case PackageStatus.NotFound:
                    return NotFound();
                case PackageStatus.Forbidden:
                    return Forbidden(result);
                default:
                    return NoContent();
            }
        }
    }
}