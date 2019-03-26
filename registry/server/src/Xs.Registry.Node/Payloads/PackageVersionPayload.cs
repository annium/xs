using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Xs.Registry.Node.Payloads
{
    public class PackageVersionPayload
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Main { get; set; }

        public Dictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> DevDependencies { get; set; } = new Dictionary<string, string>();

        [Required]
        [JsonProperty("dist")]
        public PackageDistributionPayload Distribution { get; set; }
    }
}