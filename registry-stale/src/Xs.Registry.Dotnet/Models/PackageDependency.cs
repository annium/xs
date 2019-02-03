using System;

namespace Xs.Registry.Dotnet.Models
{
    internal class PackageDependency
    {
        public Guid PackageId { get; }

        public string Framework { get; }

        public string Name { get; }

        public string Version { get; }

        internal PackageDependency(
            Guid packageId,
            string framework,
            string name,
            string version
        )
        {
            PackageId = packageId;
            Framework = framework;
            Name = name;
            Version = version;
        }
    }
}