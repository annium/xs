using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Xs.Registry.Core.Db.Models
{
    internal class UserSession
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Token { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Instant Expires { get; set; }
    }
}