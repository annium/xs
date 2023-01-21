using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Db.Shared.Entities;

[Table(nameof(Context.Users))]
internal class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public Guid ApiToken { get; set; }
}