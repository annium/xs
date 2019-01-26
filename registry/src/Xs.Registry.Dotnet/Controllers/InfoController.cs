using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Repositories;
using Xs.Registry.Dotnet.Models;

namespace Xs.Registry.Dotnet.Controllers
{
    [Route("info")]
    public class InfoController : ServerController
    {
        private readonly IPackageRepository<Package> packageRepository;

        public InfoController(
            IPackageRepository<Package> packageRepository
        )
        {
            this.packageRepository = packageRepository;
        }

        [HttpGet("search")]
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