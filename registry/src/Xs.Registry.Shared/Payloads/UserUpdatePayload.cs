using System.ComponentModel.DataAnnotations;

namespace Xs.Registry.Shared.Payloads
{
    public class UserUpdatePayload
    {
        [Required]
        [StringLength(30, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}