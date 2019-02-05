using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Db;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Models;
using Xs.Registry.Core.Tools;

namespace Xs.Registry.Shared.Controllers
{
    [Route("package")]
    public class MetaPackagePermissionsController : ServerController
    {
        private readonly IMetaPackageManager metaPackageManager;

        private readonly IMetaPackageRepository metaPackageRepository;

        public MetaPackagePermissionsController(
            IMetaPackageManager metaPackageManager,
            IMetaPackageRepository metaPackageRepository
        )
        {
            this.metaPackageManager = metaPackageManager;
            this.metaPackageRepository = metaPackageRepository;
        }

        [HttpPost("{id}/permissions")]
        [Authorize(Access.Session)]
        public async Task<IActionResult> SetPermissionAsync(
            [FromRoute] Guid id, [FromBody] MetaPackagePermission[] permissions
        )
        {
            if (permissions == null)
                return BadRequest("Pass permissions to set");

            var metaPackage = await metaPackageRepository.GetByIdAsync(id);
            if (metaPackage == null)
                return NotFound();

            // if not owner - forbidden
            var user = GetUser();
            if (metaPackageManager.GetPermissionCategory(user, metaPackage) != PermissionCategory.Owner)
                return Forbidden("You need to be package owner to manage it's permissions.");

            metaPackageManager.SetPermissions(metaPackage, permissions);
            await metaPackageRepository.UpdateAsync(metaPackage);

            return NoContent();
        }
    }
}