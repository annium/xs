using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using Xs.Core.Models;

namespace Xs.Registry.Db.Models
{
    public class MetaPackage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public ProjectType Type { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public Instant Published { get; set; }

        [Required]
        public uint Downloads { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        public User Owner { get; set; }

        public MetaPackagePermission[] Permissions { get; set; }
    }
}