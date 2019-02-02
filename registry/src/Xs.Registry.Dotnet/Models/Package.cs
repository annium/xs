using System;
using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.Versioning;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Dotnet.Models
{
    public class Package : IPackage
    {
        public string Id { get; }

        public string MetaPackageId { get; set; }

        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public IReadOnlyDictionary<NuGetFramework, IReadOnlyDictionary<string, VersionRange>> Dependencies { get; }

        public DateTime Published { get; }

        public uint Downloads { get; set; }

        internal Package(
            string name,
            NuGetVersion version,
            string description,
            IReadOnlyDictionary<NuGetFramework, IReadOnlyDictionary<string, VersionRange>> dependencies,
            DateTime published,
            uint downloads
        )
        {
            Name = name;
            Version = version.ToString();
            Description = description;
            Dependencies = dependencies;
            Published = published;
            Downloads = downloads;
        }

        internal Package(
            string id,
            string metaPackageId,
            string name,
            NuGetVersion version,
            string description,
            IReadOnlyDictionary<NuGetFramework, IReadOnlyDictionary<string, VersionRange>> dependencies,
            DateTime published,
            uint downloads
        ) : this(name, version, description, dependencies, published, downloads)
        {
            Id = id;
            MetaPackageId = metaPackageId;
        }
    }
}