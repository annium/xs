using System.ComponentModel.DataAnnotations;

namespace Server.Main.Views.Requests;

public sealed record UserLoginRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Login { get; init; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}