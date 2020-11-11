using System;
using NodaTime;

namespace Xs.RegistryClient.Main.Models
{
    public class MetaPackage
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Instant Published { get; set; }

        public int Downloads { get; set; }

        public Guid OwnerId { get; set; }

        public string Owner { get; set; } = string.Empty;

        public MetaPackagePermission[] Permissions { get; set; } = Array.Empty<MetaPackagePermission>();
    }
}