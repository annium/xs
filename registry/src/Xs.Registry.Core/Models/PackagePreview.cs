using System;

namespace Xs.Registry.Core.Models
{
    public class PackagePreview : IPackage
    {
        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public DateTime Published { get; }

        public uint Downloads { get; set; }

        public PackagePreview(IPackage package)
        {
            Name = package.Name;
            Version = package.Version;
            Description = package.Description;
            Published = package.Published;
            Downloads = package.Downloads;
        }
    }
}