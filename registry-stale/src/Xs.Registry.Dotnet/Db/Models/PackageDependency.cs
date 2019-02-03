using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xs.Registry.Dotnet.Db.Models
{
    [Table(nameof(PackageDependency), Schema = DotnetDbContext.Schema)]
    internal class PackageDependency
    {
        [Required]
        public Guid PackageId { get; set; }

        [Required]
        public string Framework { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }
    }
}