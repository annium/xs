using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Node.Domain;
using Server.Shared.Extensions;

namespace Server.Node.Views;

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
        Name = package.Name;
        Version = package.Version;
        Description = package.Description;
        Main = package.Main;
        Dependencies = package.Dependencies.Where(d => d.Type == DependencyType.Normal).ToDictionary(d => d.Name, d => d.Version);
        DevDependencies = package.Dependencies.Where(d => d.Type == DependencyType.Dev).ToDictionary(d => d.Name, d => d.Version);
        Distribution = new PackageDistributionView(
            urlHelper.AbsoluteUri($"{HttpUtility.UrlEncode(package.Name)}/{package.Version}.tgz").ToString(),
            package.Shasum,
            package.Integrity
        );
    }
}