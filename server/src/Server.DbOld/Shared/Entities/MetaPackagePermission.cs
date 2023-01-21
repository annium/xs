using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Domain.Models;

namespace Server.Db.Shared.Entities;

[Table(nameof(Context.MetaPackagePermissions))]
internal class MetaPackagePermission
{
    [Required]
    public Guid MetaPackageId { get; set; }

    [Required]
    public PermissionCategory Category { get; set; }

    [Required]
    public Permission Permission { get; set; }
}