using System;
using Microsoft.AspNetCore.Mvc;
using Xs.Core.Models;
using Xs.Registry.Db.Shared;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Controllers
{
    [Route("registry")]
    public class RegistryController : ServerController<User>
    {
        private readonly IRegistryManager registryManager;

        public RegistryController(
            IRegistryManager registryManager
        )
        {
            this.registryManager = registryManager;
        }

        [HttpGet]
        public IActionResult GetRegistries()
        {
            return Ok(registryManager.GetRegistries());
        }

        [HttpPost]
        public IActionResult Register(string type, Uri uri)
        {
            // TODO: potential vulnerability. Use private K/V storage for this
            ProjectType.Register(type);
            registryManager.AddRegistry(ProjectType.Get(type), uri);

            return NoContent();
        }
    }
}