using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Annium.Data.Operations;
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
        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        public MetaPackageController(
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository
        )
        {
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
        }

        [HttpGet("search")]
        [Authorize(Access.Api | Access.Session)]
        public async Task<IActionResult> FindPackagesAsync(
            Guid ownerId = default(Guid),
            string type = null,
            string query = null,
            int page = 1,
            int count = 50
        )
        {
            var projectType = type == null ? null : ProjectType.Get(type);
            query = HttpUtility.UrlDecode(query);
            if (page < 1)
                return BadRequest(Result.Failure().Error("Page must be positive integer"));
            if (count < 1)
                return BadRequest(Result.Failure().Error("Count must be positive integer"));

            var packages = await metaPackageRepository.FindAsync(GetUser().Id, ownerId, projectType, query, page, count);

            return Ok(packages.Select(p => new MetaPackageView(p)).ToArray());
        }

        [HttpGet("{type}/{name}")]
        [AuthorizeSession]
        public async Task<IActionResult> GetPackageAsync(string type, string name)
        {
            name = HttpUtility.UrlDecode(name);
            var package = await metaPackageRepository.FindByTypeNameAsync(ProjectType.Get(type), name);

            if (package == null)
                return NotFound();

            var access = metaPackageManager.GetAccess(package).ForUser(GetUser());
            if (!access.Has(Permission.Read))
                return Forbidden(Result.Failure().Error("You need read permission to get this package."));

            return Ok(new MetaPackageView(package));
        }

        [HttpPost("{type}/{name}/permissions")]
        [AuthorizeSession]
        public async Task<IActionResult> UpdatePackagePermissionsAsync(string type, string name, [FromBody] MetaPackagePermission[] permissions)
        {
            name = HttpUtility.UrlDecode(name);
            var package = await metaPackageRepository.FindByTypeNameAsync(ProjectType.Get(type), name);

            if (package == null)
                return NotFound();

            var access = metaPackageManager.GetAccess(package).ForUser(GetUser());
            if (!access.IsOwner)
                return Forbidden(Result.Failure().Error("You need to be owner to update package permissions."));

            await metaPackageRepository.UpdatePermissionsAsync(package.Id, permissions);

            return NoContent();
        }
    }
}