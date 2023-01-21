using System.ComponentModel.DataAnnotations;

namespace Server.Node.Payloads;

public class PackageAttachmentPayload
{
    [Required]
    public string Data { get; set; }
}