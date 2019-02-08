using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xs.Registry.Db.Node;

namespace Xs.Registry.Node.Views
{
    public class PackageView
    {
        public string Name { get; }

        public string Description { get; }

        [JsonProperty("dist-tags")]
        public IReadOnlyDictionary<string, string> DistributionTags { get; }

        public IReadOnlyDictionary<string, PackageVersionView> Versions { get; }

        public IReadOnlyDictionary<string, string> Time { get; }

        public PackageView(IEnumerable<Package> packages, IUrlHelper urlHelper)
        {
            packages = packages.OrderByDescending(e => e.Version);
            var latest = packages.First();

            Name = latest.Name.ToString();
            Description = latest.Description;
            DistributionTags = new Dictionary<string, string>() { { "latest", latest.Version } };
            Versions = packages.Select(e => new PackageVersionView(e, urlHelper)).ToDictionary(e => e.Version, e => e);

            var times = packages.ToDictionary(e => e.Version, e => e.Published.ToDateTimeUtc().ToString(Configuration.DateFormat));
            times["created"] = packages.First().Published.ToDateTimeUtc().ToString(Configuration.DateFormat);
            times["modified"] = latest.Published.ToDateTimeUtc().ToString(Configuration.DateFormat);
            Time = times;
        }
    }
}