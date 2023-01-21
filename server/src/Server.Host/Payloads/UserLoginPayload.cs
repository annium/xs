using System.ComponentModel.DataAnnotations;

namespace Server.Host.Payloads;

public class UserLoginPayload
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}