using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Core.Models
{
    public class Metadata
    {
        public string Id { get; }

        public string OwnerId { get; }

        public IReadOnlyDictionary<PermissionCategory, Permission> Permissions { get; }

        internal Metadata(
            string ownerId,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        )
        {
            this.OwnerId = ownerId;
            this.Permissions = permissions;
        }

        internal Metadata(
            string id,
            string ownerId,
            IReadOnlyDictionary<PermissionCategory, Permission> permissions
        ) : this(ownerId, permissions)
        {
            this.Id = id;
        }
    }
}