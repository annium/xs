using System.ComponentModel.DataAnnotations;

namespace Server.Node.Views.Requests;

public class PackageAttachmentRequest
{
    [Required]
    public string Data { get; set; }
}