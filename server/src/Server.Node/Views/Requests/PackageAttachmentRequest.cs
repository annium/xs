using System.ComponentModel.DataAnnotations;

namespace Server.Node.Views.Requests;

public sealed record PackageAttachmentRequest
{
    [Required]
    public string Data { get; init; } = string.Empty;
}
