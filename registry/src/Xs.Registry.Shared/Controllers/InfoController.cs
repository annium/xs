// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Xs.Registry.Core.Helpers;
// using Xs.Registry.Core.Tools;

// namespace Xs.Registry.Dotnet.Controllers
// {
//     [Route("info")]
//     public class InfoController : ServerController
//     {
//         private readonly ISearchManager searchManager;

//         public InfoController(
//             ISearchManager searchManager
//         )
//         {
//             this.searchManager = searchManager;
//         }

//         [HttpGet("search")]
//         public async Task<IActionResult> SearchAsync(string query) =>
//             Ok(await searchManager.FindPackagesAsync(query));

//         [HttpGet("{name}")]
//         public async Task<IActionResult> GetLatestAsync(string name)
//         {
//             var package = await searchManager.FindLatestPackageAsync(name);

//             if (package == null)
//                 return NotFound();

//             return Ok(package);
//         }

//         [HttpGet("{name}/{version}")]
//         public async Task<IActionResult> GetAsync(string name, string version)
//         {
//             var package = await searchManager.FindPackageAsync(name, version);

//             if (package == null)
//                 return NotFound();

//             return Ok(package);
//         }
//     }
// }