using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Server.Node.Views.Requests;

public class PackageVersionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Main { get; set; }

    public Dictionary<string, string> Dependencies { get; set; } = new();

    public Dictionary<string, string> DevDependencies { get; set; } = new();

    [Required]
    [JsonProperty("dist")]
    public PackageDistributionRequest Distribution { get; set; }
}