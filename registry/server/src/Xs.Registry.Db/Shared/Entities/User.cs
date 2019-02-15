using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xs.Registry.Db.Shared.Entities
{
    [Table(nameof(Context.Users), Schema = Schema.Shared)]
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
}