using System.Collections.Generic;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Node.Models;

namespace Xs.Registry.Node.Views
{
    public class PackageVersionView
    {
        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public string Main { get; }

        public IReadOnlyDictionary<string, string> Dependencies { get; }

        public IReadOnlyDictionary<string, string> DevDependencies { get; }

        [JsonProperty("dist")]
        public PackageDistributionView Distribution { get; }

        public PackageVersionView(Package package, IUrlHelper urlHelper)
        {
            Name = package.Name.ToString();
            Version = package.Version;
            Description = package.Description;
            Main = package.Main;
            Dependencies = package.Dependencies;
            DevDependencies = package.DevDependencies;
            Distribution = new PackageDistributionView(
                urlHelper.AbsoluteUri($"{HttpUtility.UrlEncode(package.Name.ToString())}/{package.Version}.tgz").ToString(),
                package.Shasum,
                package.Integrity
            );
        }
    }
}