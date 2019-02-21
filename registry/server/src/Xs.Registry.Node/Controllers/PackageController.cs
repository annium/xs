using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Node;
using Xs.Registry.Db.Shared;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Views;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Node.Controllers
{
    [Route("packages")]
    public class PackageController : ServerController<User>
    {
        private readonly IPackageService<Package, PackageDependency, PackagePayload> packageService;

        public PackageController(
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
            switch (result)
            {
                case Abstract.Packages.NotFoundResult res:
                    return NotFound();
                case Abstract.Packages.ForbiddenResult res:
                    return Forbidden(res.Error);
                case Abstract.Packages.ArrayResult<Package> res:
                    return Ok(res.Packages.Select(p => new PackageView(p)).ToArray());
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
            switch (result)
            {
                case Abstract.Packages.NotFoundResult res:
                    return NotFound();
                case Abstract.Packages.ForbiddenResult res:
                    return Forbidden(res.Error);
                default:
                    return NoContent();
            }
        }
    }
}