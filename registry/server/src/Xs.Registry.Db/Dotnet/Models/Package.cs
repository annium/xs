using System;
using System.Collections.Generic;
using NodaTime;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Db.Dotnet
{
    public class Package : IPackage<PackageDependency>
    {
        public Guid Id { get; }

        public Guid MetaPackageId { get; }

        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public Instant Published { get; }

        public int Downloads { get; }

        public IEnumerable<PackageDependency> Dependencies { get; }

        public Package(
            Guid metaPackageId,
            string name,
            string version,
            string description,
            Instant published,
            IEnumerable<PackageDependency> dependencies
        )
        {
            MetaPackageId = metaPackageId;
            Name = name;
            Version = version;
            Description = description;
            Published = published;
            Dependencies = dependencies;
        }

        internal Package(
            Guid id,
            Guid metaPackageId,
            string name,
            string version,
            string description,
            Instant published,
            int downloads,
            IEnumerable<PackageDependency> dependencies
        ) : this(metaPackageId, name, version, description, published, dependencies)
        {
            Id = id;
            Downloads = downloads;
        }
    }
}