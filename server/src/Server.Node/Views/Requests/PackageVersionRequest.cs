using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Server.Node.Views.Requests;

public sealed record PackageVersionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Main { get; set; } = string.Empty;

    public Dictionary<string, string> Dependencies { get; set; } = new();

    public Dictionary<string, string> DevDependencies { get; set; } = new();

    [Required]
    [JsonPropertyName("dist")]
    public PackageDistributionRequest Distribution { get; set; } = new();
}