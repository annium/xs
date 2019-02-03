using System;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Helpers;
using Xs.Core.Models;
using Xs.Registry.Core.Tools;

namespace Xs.Registry.Shared.Controllers
{
    [Route("registry")]
    public class RegistryController : ServerController
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