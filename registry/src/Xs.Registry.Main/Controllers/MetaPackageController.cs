using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Shared;
using Xs.Registry.Main.Views;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Controllers
{
    [Route("packages")]
    public class MetaPackageController : ServerController<User>
    {
        private readonly IMetaPackageRepository metaPackageRepository;

        public MetaPackageController(
            IMetaPackageRepository metaPackageRepository
        )
        {
            this.metaPackageRepository = metaPackageRepository;
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyPackagesAsync()
        {
            var packages = await metaPackageRepository.FindAllByOwnerIdAsync(GetUser().Id);

            return Ok(packages.Select(p => new MetaPackageView(p)).ToArray());
        }
    }
}