using System.ComponentModel.DataAnnotations;

namespace Server.Node.Payloads;

public class PackageDistributionPayload
{
    [Required]
    public string Shasum { get; set; }

    [Required]
    public string Integrity { get; set; }
}