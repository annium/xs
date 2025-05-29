using System.ComponentModel.DataAnnotations;

namespace Annium.Xs.Server.Node.Views.Requests;

public sealed record PackageDistributionRequest
{
    [Required]
    public string Shasum { get; set; } = string.Empty;

    [Required]
    public string Integrity { get; set; } = string.Empty;
}
