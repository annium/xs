using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Dotnet.Views;

namespace Xs.Registry.Dotnet.Controllers
{
    public class IndexController : ServerController
    {
        private readonly IUrlHelper url;

        public IndexController(
            IUrlHelper url
        )
        {
            this.url = url;
        }

        [HttpGet("v3/index.json")]
        public IActionResult GetIndexAsync()
        {
            var resources = new List<ServiceIndexResourceView>();

            resources.Add(new ServiceIndexResourceView { Type = "PackagePublish/2.0.0", Uri = url.AbsoluteUri("api/v2/package") });
            resources.Add(new ServiceIndexResourceView { Type = "SymbolPackagePublish/4.9.0", Uri = url.AbsoluteUri("api/v2/symbol") });
            resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl", Uri = url.AbsoluteUri("v3/registration") });
            resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl/3.0.0-beta", Uri = url.AbsoluteUri("v3/registration") });
            resources.Add(new ServiceIndexResourceView { Type = "RegistrationsBaseUrl/3.0.0-rc", Uri = url.AbsoluteUri("v3/registration") });
            resources.Add(new ServiceIndexResourceView { Type = "PackageBaseAddress", Uri = url.AbsoluteUri("v3/package") });

            return Ok(new { version = "3.0.0", resources });
        }
    }
}