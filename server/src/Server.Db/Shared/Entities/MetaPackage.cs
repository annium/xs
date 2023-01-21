using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Db.Shared.Entities;

[Table(nameof(Context.MetaPackages))]
internal class MetaPackage
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    public string Type { get; set; }

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

    [Required]
    public Guid OwnerId { get; set; }

    public User Owner { get; set; }

    public List<MetaPackagePermission> Permissions { get; set; }
}