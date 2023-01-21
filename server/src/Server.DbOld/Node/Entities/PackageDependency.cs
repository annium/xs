using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Db.Node.Models;
using Server.Db.Shared.Entities;

namespace Server.Db.Node.Entities;

[Table("NodePackageDependencies")]
internal class PackageDependency : IPackageDependency
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