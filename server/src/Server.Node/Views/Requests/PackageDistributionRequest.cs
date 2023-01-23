using System.ComponentModel.DataAnnotations;

namespace Server.Node.Views.Requests;

public class PackageDistributionRequest
{
    [Required]
    public string Shasum { get; set; }

    [Required]
    public string Integrity { get; set; }
}