using System.Collections.Generic;
using System.IO;
using NodaTime;
using Xs.Registry.Core.Models;
using Xs.Registry.Dotnet.Models;

namespace Xs.Registry.Dotnet.Payloads
{
    internal class PackagePayload : IPackageBase
    {
        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public Instant Published { get; }

        public IEnumerable<PackageDependency> Dependencies { get; }

        public Stream PackageStream { get; }

        public Stream NuspecStream { get; }

        internal PackagePayload(
            string name,
            string version,
            string description,
            Instant published,
            IEnumerable<PackageDependency> dependencies,
            Stream packageStream,
            Stream nuspecStream
        )
        {
            Name = name;
            Version = version;
            Description = description;
            Published = published;
            Dependencies = dependencies;
            PackageStream = packageStream;
            NuspecStream = nuspecStream;
        }
    }
}