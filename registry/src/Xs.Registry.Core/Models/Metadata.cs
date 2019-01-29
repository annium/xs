using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Core.Models
{
    public class Metadata
    {
        public string Id { get; }

        public string OwnerId { get; }

        public ProjectType ProjectType { get; }

        public string PackageName { get; }

        public IReadOnlyDictionary<PermissionCategory, Permission> Permissions { get; }

        internal Metadata(
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

        internal Metadata(
            string id,
            string ownerId,
            ProjectType projectType,
            string packageName,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        ) : this(ownerId, projectType, packageName, permissions)
        {
            this.Id = id;
        }
    }
}