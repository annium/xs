using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Core.Models;
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
            ProjectType.Register("dotnet");
            this.metadataManager = metadataManager;
            this.metadataRepository = metadataRepository;
        }

        [HttpGet("{type}/{name}/permissions")]
        [Authorize]
        public async Task<IActionResult> GetPermissionsAsync(
            string type,
            string name
        )
        {
            var(metadata, readResult) = await GetMetadataAsync(type, name);
            if (readResult != null)
                return readResult;

            return Ok(metadata.Permissions);
        }

        [HttpPut("{type}/{name}/permissions/{category}/{permission}")]
        [Authorize]
        public async Task<IActionResult> GrantPermissionAsync(
            string type,
            string name,
            PermissionCategory category,
            Permission permission
        )
        {
            var(metadata, readResult) = await GetMetadataAsync(type, name);
            if (readResult != null)
                return readResult;

            metadata = metadataManager.GrantPermission(metadata, category, permission);

            await metadataRepository.SaveAsync(metadata);

            return NoContent();
        }

        [HttpDelete("{type}/{name}/permissions/{category}/{permission}")]
        [Authorize]
        public async Task<IActionResult> RevokePermissionAsync(
            string type,
            string name,
            PermissionCategory category,
            Permission permission
        )
        {
            var(metadata, readResult) = await GetMetadataAsync(type, name);
            if (readResult != null)
                return readResult;

            metadata = metadataManager.RevokePermission(metadata, category, permission);

            await metadataRepository.SaveAsync(metadata);

            return NoContent();
        }

        private async Task<ValueTuple<Metadata, IActionResult>> GetMetadataAsync(string type, string name)
        {
            var projectType = ProjectType.Get(type);
            var user = GetUser();

            var metadata = await metadataRepository.FindByProjectTypePackageNameAsync(projectType, name);
            if (metadata == null)
                return (null, NotFound());

            // if not owner - forbidden
            if (metadataManager.GetPermissionCategory(user, metadata) != PermissionCategory.Owner)
                return (null, Forbidden("You need to be package owner to manage it's permissions"));

            return (metadata, null);
        }
    }
}