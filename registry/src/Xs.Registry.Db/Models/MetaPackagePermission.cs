using System;
using System.ComponentModel.DataAnnotations;
using Xs.Core.Models;

namespace Xs.Registry.Db.Models
{
    public class MetaPackagePermission
    {
        [Required]
        public Guid MetaPackageId { get; set; }

        [Required]
        public PermissionCategory Category { get; set; }

        [Required]
        public Permission Permission { get; set; }
    }
}