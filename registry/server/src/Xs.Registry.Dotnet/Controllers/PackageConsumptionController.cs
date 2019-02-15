using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Storage;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackageConsumptionController : ServerController<User>
    {
        private readonly IMetaPackageRepository metaPackageRepository;

        private readonly IPackageRepository<Package> packageRepository;

        private readonly IPackageStorage packageStorage;

        public PackageConsumptionController(
            IMetaPackageRepository metaPackageRepository,
            IPackageRepository<Package> packageRepository,
            IPackageStorage packageStorage
        )
        {
            this.metaPackageRepository = metaPackageRepository;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
        }

        [HttpGet("v3/package/{name}/index.json")]
        public async Task<IActionResult> GetVersionsAsync(string name, CancellationToken token)
        {
            var versions = await packageRepository.FindAllVersionsByNameAsync(name);

            if (versions.Length == 0)
                return NotFound();

            return Ok(new { versions });
        }

        [HttpGet("v3/package/{name}/{version}/{name2}.{version2}.nupkg")]
        public async Task<IActionResult> DownloadPackageAsync(string name, string version, CancellationToken token)
        {
            var package = await packageRepository.FindByNameVersionAsync(name, version);

            if (package == null)
                return NotFound();

            if (!(await packageStorage.ExistsAsync(name, version)))
                return ServerError("Package file missing");

            await packageRepository.IncrementDownloadsAsync(package.Id);
            var total = await packageRepository.CountAllDownloadsAsync(package.Name);
            await metaPackageRepository.SetDownloadsAsync(package.MetaPackageId, total);

            var content = await packageStorage.GetPackageAsync(name, version);

            return File(content, "application/octet-stream");
        }

        [HttpGet("v3/package/{name}/{version}/{name2}.nuspec")]
        public async Task<IActionResult> DownloadNuspecAsync(string name, string version, CancellationToken token)
        {
            if ((await packageRepository.FindByNameVersionAsync(name, version)) == null)
                return NotFound();

            if (!(await packageStorage.ExistsAsync(name, version)))
                return ServerError("Nuspec file missing");

            var content = await packageStorage.GetNuspecAsync(name, version);

            return File(content, "text/xml");
        }
    }
}