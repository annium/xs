using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Core.Models;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Models;
using Xs.Registry.Core.Repositories;
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

        [HttpGet("{id}/permissions")]
        [Authorize(Access.Session)]
        public async Task<IActionResult> GetPermissionsAsync(
            string id
        )
        {
            var(metaPackage, readResult) = await GetMetaPackageAsync(id);
            if (readResult != null)
                return readResult;

            return Ok(metaPackage.Permissions);
        }

        [HttpPost("{id}/permissions")]
        [Authorize(Access.Session)]
        public async Task<IActionResult> SetPermissionAsync(
            string id, [FromBody] Dictionary<PermissionCategory, Permission> permissions
        )
        {
            if (permissions == null)
                return BadRequest("Pass permissions to set");

            var(metaPackage, readResult) = await GetMetaPackageAsync(id);
            if (readResult != null)
                return readResult;

            metaPackage = metaPackageManager.SetPermissions(metaPackage, permissions);

            await metaPackageRepository.SaveAsync(metaPackage);

            return NoContent();
        }

        private async Task<ValueTuple<MetaPackage, IActionResult>> GetMetaPackageAsync(string id)
        {
            var user = GetUser();

            var metaPackage = await metaPackageRepository.GetByIdAsync(id);
            if (metaPackage == null)
                return (null, NotFound());

            // if not owner - forbidden
            if (metaPackageManager.GetPermissionCategory(user, metaPackage) != PermissionCategory.Owner)
                return (null, Forbidden("You need to be package owner to manage it's permissions."));

            return (metaPackage, null);
        }
    }
}