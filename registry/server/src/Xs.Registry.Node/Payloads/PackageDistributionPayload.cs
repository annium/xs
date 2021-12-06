using System.ComponentModel.DataAnnotations;

namespace Xs.Registry.Node.Payloads;

public class PackageDistributionPayload
{
    [Required]
    public string Shasum { get; set; }

    [Required]
    public string Integrity { get; set; }
}