using System;
using System.Collections.Generic;
using NodaTime;

namespace Xs.Registry.Db.Shared
{
    public class MetaPackage
    {
        public Guid Id { get; }

        public ProjectType Type { get; }

        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public Instant Published { get; }

        public int Downloads { get; }

        public Guid OwnerId { get; }

        public User Owner { get; }

        public IEnumerable<MetaPackagePermission> Permissions { get; }

        internal MetaPackage(
            ProjectType type,
            string name,
            string version,
            string description,
            Instant published,
            int downloads,
            Guid ownerId,
            User owner,
            IEnumerable<MetaPackagePermission> permissions
        )
        {
            Type = type;
            Name = name;
            Version = version;
            Description = description;
            Published = published;
            Downloads = downloads;
            OwnerId = ownerId;
            Owner = owner;
            Permissions = permissions;
        }

        internal MetaPackage(
            Guid id,
            ProjectType type,
            string name,
            string version,
            string description,
            Instant published,
            int downloads,
            Guid ownerId,
            User owner,
            IEnumerable<MetaPackagePermission> permissions
        ) : this(type, name, version, description, published, downloads, ownerId, owner, permissions)
        {
            Id = id;
        }
    }
}