using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Core.Models
{
    public class Metadata
    {
        public string UserId { get; }

        public ProjectType ProjectType { get; }

        public string PackageName { get; }

        public IReadOnlyDictionary<PermissionCategory, Permission> Permissions { get; }

        public Metadata(
            string userId,
            ProjectType projectType,
            string packageName,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        )
        {
            this.UserId = userId;
            this.ProjectType = projectType;
            this.PackageName = packageName;
            this.Permissions = permissions;
        }
    }
}