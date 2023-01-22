using System.ComponentModel.DataAnnotations;

namespace Server.Main.Payloads;

public class UserRegistrationPayload
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}