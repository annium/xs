// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Xs.Registry.Core.Helpers;
// using Xs.Registry.Core.Db;
// using Xs.Registry.Dotnet.Models;
// using Xs.Registry.Dotnet.Storage;

// namespace Xs.Registry.Dotnet.Controllers
// {
//     public class PackageConsumptionController : ServerController
//     {
//         private readonly IPackageRepository<Package> packageRepository;

//         private readonly IPackageStorage packageStorage;

//         public PackageConsumptionController(
//             IPackageRepository<Package> packageRepository,
//             IPackageStorage packageStorage
//         )
//         {
//             this.packageRepository = packageRepository;
//             this.packageStorage = packageStorage;
//         }

//         [HttpGet("v3/package/{name}/index.json")]
//         public async Task<IActionResult> GetVersionsAsync(string name, CancellationToken token)
//         {
//             var packages = await packageRepository.FindAllByNameAsync(name);

//             if (packages.Length == 0)
//                 return NotFound();

//             var versions = packages.Select(e => e.Version).ToArray();

//             return Ok(new { versions });
//         }

//         [HttpGet("v3/package/{name}/{version}/{name2}.{version2}.nupkg")]
//         public async Task<IActionResult> DownloadPackageAsync(string name, string version, CancellationToken token)
//         {
//             var package = await packageRepository.FindByNameVersionAsync(name, version);

//             if (package == null)
//                 return NotFound();

//             if (!(await packageStorage.ExistsAsync(name, version)))
//                 return ServerError("Package file missing");

//             package.Downloads++;
//             await packageRepository.SaveAsync(package);

//             var content = await packageStorage.GetPackageAsync(name, version);

//             return File(content, "application/octet-stream");
//         }

//         [HttpGet("v3/package/{name}/{version}/{name2}.nuspec")]
//         public async Task<IActionResult> DownloadNuspecAsync(string name, string version, CancellationToken token)
//         {
//             if ((await packageRepository.FindByNameVersionAsync(name, version)) == null)
//                 return NotFound();

//             if (!(await packageStorage.ExistsAsync(name, version)))
//                 return ServerError("Nuspec file missing");

//             var content = await packageStorage.GetNuspecAsync(name, version);

//             return File(content, "text/xml");
//         }
//     }
// }