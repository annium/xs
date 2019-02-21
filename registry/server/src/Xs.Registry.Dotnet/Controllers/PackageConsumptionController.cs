using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Payloads;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackageConsumptionController : ServerController<User>
    {
        private readonly IPackageService<Package, PackageDependency, PackagePayload> packageService;

        private readonly IPackageRepository<Package, PackageDependency> packageRepository;

        private readonly Storage.IPackageStorage packageStorage;

        public PackageConsumptionController(
            IPackageService<Package, PackageDependency, PackagePayload> packageService,
            IPackageRepository<Package, PackageDependency> packageRepository,
            Storage.IPackageStorage packageStorage
        )
        {
            this.packageService = packageService;
            this.packageRepository = packageRepository;
            this.packageStorage = packageStorage;
        }

        [HttpGet("v3/package/{name}/index.json")]
        public async Task<IActionResult> GetVersionsAsync(string name, CancellationToken token)
        {
            name = HttpUtility.UrlDecode(name);
            var versions = await packageRepository.FindAllVersionsByNameAsync(name);

            if (versions.Length == 0)
                return NotFound();

            return Ok(new { versions });
        }

        [HttpGet("v3/package/{name}/{version}/{name2}.{version2}.nupkg")]
        public async Task<IActionResult> DownloadPackageAsync(string name, string version, CancellationToken token)
        {
            name = HttpUtility.UrlDecode(name);
            var result = await packageService.ProcessDownloadAsync(null, name, version, true);
            switch (result)
            {
                case Abstract.Packages.NotFoundResult res:
                    return NotFound();
                case Abstract.Packages.ForbiddenResult res:
                    return Forbidden(res.Error);
                case Abstract.Packages.InternalErrorResult res:
                    return ServerError(res.Error);
            }

            var content = await packageStorage.GetPackageAsync(name, version);

            return File(content, "application/octet-stream");
        }

        [HttpGet("v3/package/{name}/{version}/{name2}.nuspec")]
        public async Task<IActionResult> DownloadNuspecAsync(string name, string version, CancellationToken token)
        {
            name = HttpUtility.UrlDecode(name);
            var result = await packageService.ProcessDownloadAsync(null, name, version, false);
            switch (result)
            {
                case Abstract.Packages.NotFoundResult res:
                    return NotFound();
                case Abstract.Packages.ForbiddenResult res:
                    return Forbidden(res.Error);
                case Abstract.Packages.InternalErrorResult res:
                    return ServerError(res.Error);
            }

            var content = await packageStorage.GetNuspecAsync(name, version);

            return File(content, "text/xml");
        }
    }
}