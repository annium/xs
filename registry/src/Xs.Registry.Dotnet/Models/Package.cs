using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace Xs.Registry.Dotnet.Models
{
    public class Package
    {
        public string Name { get; }

        public NuGetVersion Version { get; }

        public string Description { get; }

        public IReadOnlyDictionary<NuGetFramework, IReadOnlyDictionary<string, VersionRange>> Dependencies { get; }

        public Package(
            string name,
            NuGetVersion version,
            string description,
            IReadOnlyDictionary<NuGetFramework, IReadOnlyDictionary<string, VersionRange>> dependencies
        )
        {
            Name = name;
            Version = version;
            Description = description;
            Dependencies = dependencies;
        }
    }
}