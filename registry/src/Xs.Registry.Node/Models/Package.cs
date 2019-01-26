using System;
using System.Collections.Generic;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Node.Models
{
    public class Package : IPackage
    {
        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public string Main { get; }

        public IReadOnlyDictionary<string, string> Dependencies { get; }

        public IReadOnlyDictionary<string, string> DevDependencies { get; }

        public DateTime Published { get; }

        public string Shasum { get; }

        public string Integrity { get; }

        public Package(
            PackageName name,
            string version,
            string description,
            string main,
            IReadOnlyDictionary<string, string> dependencies,
            IReadOnlyDictionary<string, string> devDependencies,
            DateTime published,
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
            Shasum = shasum;
            Integrity = integrity;
        }
    }
}