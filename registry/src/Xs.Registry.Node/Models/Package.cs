using System;
using System.Collections.Generic;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Node.Models
{
    public class Package : IPackage
    {
        public string Id { get; }

        public string MetadataId { get; set; }

        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public string Main { get; }

        public IReadOnlyDictionary<string, string> Dependencies { get; }

        public IReadOnlyDictionary<string, string> DevDependencies { get; }

        public DateTime Published { get; }

        public uint Downloads { get; set; }

        public string Shasum { get; }

        public string Integrity { get; }

        internal Package(
            PackageName name,
            string version,
            string description,
            string main,
            IReadOnlyDictionary<string, string> dependencies,
            IReadOnlyDictionary<string, string> devDependencies,
            DateTime published,
            uint downloads,
            string shasum,
            string integrity
        )
        {
            Name = name;
            Version = version;
            Description = description;
            Main = main;
            Dependencies = dependencies;
            DevDependencies = devDependencies;
            Published = published;
            Downloads = downloads;
            Shasum = shasum;
            Integrity = integrity;
        }

        internal Package(
            string id,
            string metadataId,
            PackageName name,
            string version,
            string description,
            string main,
            IReadOnlyDictionary<string, string> dependencies,
            IReadOnlyDictionary<string, string> devDependencies,
            DateTime published,
            uint downloads,
            string shasum,
            string integrity
        ) : this(name, version, description, main, dependencies, devDependencies, published, downloads, shasum, integrity)
        {
            Id = id;
            MetadataId = metadataId;
        }
    }
}