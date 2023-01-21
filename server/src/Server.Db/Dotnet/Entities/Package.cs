using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xs.Registry.Db.Shared.Entities;

namespace Xs.Registry.Db.Dotnet.Entities;

[Table("DotnetPackages")]
internal class Package : IPackage<PackageDependency>
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    public Guid MetaPackageId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string LowerName { get; set; }

    [Required]
    public string Version { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public DateTime Published { get; set; }

    [Required]
    public int Downloads { get; set; }

    public List<PackageDependency> Dependencies { get; set; }
}