using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Db.Shared.Entities;

[Table(nameof(Context.UserSessions))]
internal class UserSession
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Required]
    public Guid Token { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime Expires { get; set; }
}