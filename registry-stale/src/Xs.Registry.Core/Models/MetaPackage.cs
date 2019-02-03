using System;
using System.Collections.Generic;
using NodaTime;
using Xs.Core.Models;

namespace Xs.Registry.Core.Models
{
    public class MetaPackage
    {
        public Guid Id { get; } = Guid.NewGuid();

        public ProjectType Type { get; }

        public string Name { get; internal set; }

        public string Version { get; internal set; }

        public string Description { get; internal set; }

        public Instant Published { get; internal set; }

        public uint Downloads { get; }

        public Guid OwnerId { get; }

        public User Owner { get; }

        public IEnumerable<MetaPackagePermission> Permissions { get; internal set; } = Array.Empty<MetaPackagePermission>();

        internal MetaPackage(
            ProjectType type,
            string name,
            string version,
            string description,
            Instant published,
            uint downloads,
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
            uint downloads,
            Guid ownerId,
            User owner,
            IEnumerable<MetaPackagePermission> permissions
        ) : this(type, name, version, description, published, downloads, ownerId, owner, permissions)
        {
            Id = id;
        }
    }
}