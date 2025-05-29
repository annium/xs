using System.ComponentModel.DataAnnotations;

namespace Annium.Xs.Server.Node.Views.Requests;

public sealed record PackageAttachmentRequest
{
    [Required]
    public string Data { get; init; } = string.Empty;
}
