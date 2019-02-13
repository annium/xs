using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xs.Registry.Db.Shared.Entities;

namespace Xs.Registry.Db.Dotnet.Entities
{
    [Table(nameof(PackageDependency), Schema = Schema.Dotnet)]
    internal class PackageDependency : IPackageDependency
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