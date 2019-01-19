using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Xs.Registry.Node.Payloads
{
    public class PackageVersionPayload
    {
        [StringLength(100, MinimumLength = 5)]
        public string Main { get; set; }

        public Dictionary<string, string> Dependencies { get; set; }

        public Dictionary<string, string> DevDependencies { get; set; }

        [Required]
        [JsonProperty("dist")]
        public PackageDistributionPayload Distribution { get; set; }
    }
}