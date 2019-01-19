using System.ComponentModel.DataAnnotations;

namespace Xs.Registry.Node.Payloads
{
    public class PackageAttachmentPayload
    {
        [Required]
        public string Data { get; set; }
    }
}