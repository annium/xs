using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Db.Dotnet.Entities
{
    [Table(nameof(Package), Schema = Schema.Dotnet)]
    internal class Package : IPackageInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid MetaPackageId { get; set; }

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

        public List<PackageDependency> Dependencies { get; set; }
    }
}