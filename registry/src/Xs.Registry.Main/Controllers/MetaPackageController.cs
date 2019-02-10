using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> FindPackagesAsync(string query = "", int page = 1, int count = 50)
        {
            query = HttpUtility.UrlDecode(query);
            if (page < 1)
                return BadRequest("Page must be positive integer");

            if (count < 1)
                return BadRequest("Count must be positive integer");

            var packages = await metaPackageRepository.FindPackagesByQueryAsync(GetUser().Id, query, page, count);

            return Ok(packages.Select(p => new MetaPackageView(p)).ToArray());
        }

        [HttpGet("{type}/{name}")]
        [Authorize]
        public async Task<IActionResult> GetPackageAsync(string type, string name)
        {
            name = HttpUtility.UrlDecode(name);
            var package = await metaPackageRepository.FindByTypeNameAsync(ProjectType.Get(type), name);

            if (package == null)
                return NotFound();

            return Ok(new MetaPackageView(package));
        }
    }
}