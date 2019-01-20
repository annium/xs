using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Node.Repositories;

namespace Xs.Registry.Node.Controllers
{
    [Route("info")]
    public class InfoController : ServerController
    {
        private readonly IPackageRepository packageRepository;

        public InfoController(
            IPackageRepository packageRepository
        )
        {
            this.packageRepository = packageRepository;
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchAsync(string query)
        {
            var packages = (await packageRepository.FindAllByQueryAsync(query)).OrderByDescending(e => e.Version);

            var result = new Dictionary<string, string>();

            foreach (var package in packages)
                if (!result.ContainsKey(package.Name))
                    result[package.Name] = package.Version;

            return Ok(result);
        }

        [HttpGet("{name}")]
        [Authorize]
        public async Task<IActionResult> InfoAsync(string name)
        {
            var package = (await packageRepository.FindAllByNameAsync(name))
                .OrderByDescending(e => e.Version).FirstOrDefault();
            if (package == null)
                return NotFound();

            return Ok(package);
        }
    }
}