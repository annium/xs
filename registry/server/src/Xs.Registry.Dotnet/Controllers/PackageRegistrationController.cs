using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Views;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet.Controllers
{
    public class PackageRegistrationController : ServerController<User>
    {
        private readonly IPackageRepository<Package, PackageDependency> packageRepository;

        private readonly IUrlHelper url;

        public PackageRegistrationController(
            IPackageRepository<Package, PackageDependency> packageRepository,
            IUrlHelper url,
            IMediator mediator
        ) : base(mediator)
        {
            this.packageRepository = packageRepository;
            this.url = url;
        }

        [HttpGet("v3/registration/{name}/index.json")]
        public async Task<IActionResult> GetRegistrationIndexAsync(string name, CancellationToken token)
        {
            var packages = await packageRepository.FindAllByNameAsync(name);

            if (packages.Length == 0)
                return NotFound();

            return Ok(new RegistrationIndexView(new [] { GetRegistrationPage(packages) }));
        }

        [HttpGet("v3/registration/{name}/page.json")]
        public async Task<IActionResult> GetRegistrationPageAsync(string name, CancellationToken token)
        {
            var packages = await packageRepository.FindAllByNameAsync(name);

            if (packages.Length == 0)
                return NotFound();

            return Ok(GetRegistrationPage(packages));
        }

        [HttpGet("v3/registration/{name}/{version}/leaf.json")]
        public async Task<IActionResult> GetRegistrationLeafAsync(string name, string version, CancellationToken token)
        {
            var package = await packageRepository.FindByNameVersionAsync(name, version);

            if (package == null)
                return NotFound();

            return Ok(GetRegistrationLeaf(package));
        }

        [HttpGet("v3/registration/{name}/{version}/catalog-entry.json")]
        public async Task<IActionResult> GetCatalogEntryAsync(string name, string version, CancellationToken token)
        {
            var package = await packageRepository.FindByNameVersionAsync(name, version);

            if (package == null)
                return NotFound();

            return Ok(GetCatalogEntry(package));
        }

        private RegistrationPageView GetRegistrationPage(Package[] packages)
        {
            var id = packages.First().Name.ToLowerInvariant();
            var leafs = packages.Select(GetRegistrationLeaf).ToArray();
            var lower = packages.Min(e => e.Version);
            var upper = packages.Max(e => e.Version);

            return new RegistrationPageView(url.AbsoluteUri($"v3/registration/{id}/page.json"), leafs, lower, upper);
        }

        private RegistrationLeafView GetRegistrationLeaf(Package package)
        {
            var id = package.Name.ToLowerInvariant();
            var version = package.Version;

            return new RegistrationLeafView(
                url.AbsoluteUri($"v3/registration/{id}/{version}/leaf.json"),
                GetCatalogEntry(package),
                url.AbsoluteUri($"v3/package/{id}/{version}/{id}.{version}.nupkg")
            );
        }

        private CatalogEntryView GetCatalogEntry(Package package)
        {
            var id = package.Name.ToLowerInvariant();
            var name = package.Name;
            var version = package.Version;

            return new CatalogEntryView(url.AbsoluteUri($"v3/registration/{id}/{version}/catalog-entry.json"), name, version);
        }
    }
}