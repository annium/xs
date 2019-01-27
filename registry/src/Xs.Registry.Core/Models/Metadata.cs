using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Core.Models
{
    public class Metadata
    {
        public string OwnerId { get; }

        public ProjectType ProjectType { get; }

        public string PackageName { get; }

        public IReadOnlyDictionary<PermissionCategory, Permission> Permissions { get; }

        public Metadata(
            string ownerId,
            ProjectType projectType,
            string packageName,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        )
        {
            this.OwnerId = ownerId;
            this.ProjectType = projectType;
            this.PackageName = packageName;
            this.Permissions = permissions;
        }
    }
}