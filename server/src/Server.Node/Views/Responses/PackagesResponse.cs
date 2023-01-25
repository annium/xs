using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Server.Abstractions.Tools;
using Server.Node.Domain;

namespace Server.Node.Views.Responses;

public sealed record PackagesResponse
{
    public string Name { get; }
    public string Description { get; }

    [JsonPropertyName("dist-tags")]
    public IReadOnlyDictionary<string, string> DistributionTags { get; }

    public IReadOnlyDictionary<string, PackageVersionResponse> Versions { get; }
    public IReadOnlyDictionary<string, string> Time { get; }

    public PackagesResponse(IReadOnlyCollection<Package> packages, IUrlTool urlTool)
    {
        packages = packages.OrderByDescending(e => e.Version).ToArray();
        var latest = packages.First();

        Name = latest.Name;
        Description = latest.Description;
        DistributionTags = new Dictionary<string, string>() { { "latest", latest.Version } };
        Versions = packages.Select(e => new PackageVersionResponse(e, urlTool)).ToDictionary(e => e.Version, e => e);

        var times = packages.ToDictionary(e => e.Version, e => e.Published.InUtc().ToString(Configuration.DateFormat, null));
        times["created"] = packages.First().Published.InUtc().ToString(Configuration.DateFormat, null);
        times["modified"] = latest.Published.InUtc().ToString(Configuration.DateFormat, null);
        Time = times;
    }
}