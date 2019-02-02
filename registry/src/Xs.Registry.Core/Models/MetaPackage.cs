using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Core.Models
{
    public class MetaPackage
    {
        public string Id { get; }

        public string OwnerId { get; }

        public IReadOnlyDictionary<PermissionCategory, Permission> Permissions { get; }

        internal MetaPackage(
            string ownerId,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        )
        {
            this.OwnerId = ownerId;
            this.Permissions = permissions;
        }

        internal MetaPackage(
            string id,
            string ownerId,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        ) : this(ownerId, permissions)
        {
            this.Id = id;
        }
    }
}