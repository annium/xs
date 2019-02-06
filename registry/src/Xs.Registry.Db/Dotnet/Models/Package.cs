using System;
using System.Collections.Generic;
using NodaTime;

namespace Xs.Registry.Db.Dotnet
{
    public class Package
    {
        public Guid Id { get; }

        public Guid MetaPackageId { get; }

        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public Instant Published { get; }

        public uint Downloads { get; }

        public IEnumerable<PackageDependency> Dependencies { get; }

        public Package(
            Guid metaPackageId,
            string name,
            string version,
            string description,
            Instant published,
            uint downloads,
            IEnumerable<PackageDependency> dependencies
        )
        {
            MetaPackageId = metaPackageId;
            Name = name;
            Version = version;
            Description = description;
            Published = published;
            Downloads = downloads;
            Dependencies = dependencies;
        }

        internal Package(
            Guid id,
            Guid metaPackageId,
            string name,
            string version,
            string description,
            Instant published,
            uint downloads,
            IEnumerable<PackageDependency> dependencies
        ) : this(metaPackageId, name, version, description, published, downloads, dependencies)
        {
            Id = id;
        }
    }
}