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
    [Route("metadata")]
    public class MetadataPermissionsController : ServerController
    {
        private readonly IMetadataManager metadataManager;

        private readonly IMetadataRepository metadataRepository;

        public MetadataPermissionsController(
            IMetadataManager metadataManager,
            IMetadataRepository metadataRepository
        )
        {
            this.metadataManager = metadataManager;
            this.metadataRepository = metadataRepository;
        }

        [HttpGet("{id}/permissions")]
        [Authorize(Access.Session)]
        public async Task<IActionResult> GetPermissionsAsync(
            string id
        )
        {
            var(metadata, readResult) = await GetMetadataAsync(id);
            if (readResult != null)
                return readResult;

            return Ok(metadata.Permissions);
        }

        [HttpPost("{id}/permissions")]
        [Authorize(Access.Session)]
        public async Task<IActionResult> SetPermissionAsync(
            string id, [FromBody] Dictionary<PermissionCategory, Permission> permissions
        )
        {
            if (permissions == null)
                return BadRequest("Pass permissions to set");

            var(metadata, readResult) = await GetMetadataAsync(id);
            if (readResult != null)
                return readResult;

            metadata = metadataManager.SetPermissions(metadata, permissions);

            await metadataRepository.SaveAsync(metadata);

            return NoContent();
        }

        private async Task<ValueTuple<Metadata, IActionResult>> GetMetadataAsync(string id)
        {
            var user = GetUser();

            var metadata = await metadataRepository.GetByIdAsync(id);
            if (metadata == null)
                return (null, NotFound());

            // if not owner - forbidden
            if (metadataManager.GetPermissionCategory(user, metadata) != PermissionCategory.Owner)
                return (null, Forbidden("You need to be package owner to manage it's permissions."));

            return (metadata, null);
        }
    }
}