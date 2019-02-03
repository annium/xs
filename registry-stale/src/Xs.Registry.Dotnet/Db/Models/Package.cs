using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Dotnet.Db.Models
{
    [Table(nameof(Package), Schema = DotnetDbContext.Schema)]
    internal class Package : IPackage
    {
        [Key]
        public Guid Id { get; set; }

        public Guid MetaPackageId { get; set; }

        public Core.Db.Models.MetaPackage MetaPackage { get; set; }

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

        public PackageDependency[] Dependencies { get; set; }
    }
}