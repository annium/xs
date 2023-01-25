using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using Server.Abstractions.Tools;
using Server.Node.Domain;

namespace Server.Node.Views.Responses;

public sealed record PackageVersionResponse
{
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public string Main { get; }
    public IReadOnlyDictionary<string, string> Dependencies { get; }
    public IReadOnlyDictionary<string, string> DevDependencies { get; }

    [JsonPropertyName("dist")]
    public PackageDistributionResponse Distribution { get; }

    public PackageVersionResponse(Package package, IUrlTool urlTool)
    {
        Name = package.Name;
        Version = package.Version;
        Description = package.Description;
        Main = package.Main;
        Dependencies = package.Dependencies.Where(d => d.Type == DependencyType.Normal).ToDictionary(d => d.Name, d => d.Version);
        DevDependencies = package.Dependencies.Where(d => d.Type == DependencyType.Dev).ToDictionary(d => d.Name, d => d.Version);
        Distribution = new PackageDistributionResponse(
            urlTool.AbsoluteUrl($"{HttpUtility.UrlEncode(package.Name)}/{package.Version}.tgz").ToString(),
            package.Shasum,
            package.Integrity
        );
    }
}