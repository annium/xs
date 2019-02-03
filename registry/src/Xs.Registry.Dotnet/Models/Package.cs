using System;
using System.Collections.Generic;
using NodaTime;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Dotnet.Models
{
    internal class Package : IPackage
    {
        public Guid Id { get; } = Guid.NewGuid();

        public Guid MetaPackageId { get; }

        public MetaPackage MetaPackage { get; }

        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public Instant Published { get; }

        public uint Downloads { get; }

        public IEnumerable<PackageDependency> Dependencies { get; }

        internal Package(
            Guid metaPackageId,
            MetaPackage metaPackage,
            string name,
            string version,
            string description,
            Instant published,
            uint downloads,
            IEnumerable<PackageDependency> dependencies
        )
        {
            MetaPackageId = metaPackageId;
            MetaPackage = metaPackage;
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
            MetaPackage metaPackage,
            string name,
            string version,
            string description,
            Instant published,
            uint downloads,
            IEnumerable<PackageDependency> dependencies
        ) : this(metaPackageId, metaPackage, name, version, description, published, downloads, dependencies)
        {
            Id = id;
        }
    }
}