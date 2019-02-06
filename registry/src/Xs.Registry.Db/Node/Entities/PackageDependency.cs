using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xs.Registry.Db.Node.Entities
{
    [Table(nameof(PackageDependency), Schema = Schema.Node)]
    internal class PackageDependency
    {
        [Required]
        public Guid PackageId { get; set; }

        [Required]
        public DependencyType Type { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }
    }
}