using System;
using NodaTime;

namespace Xs.RegistryClient.Main.Models
{
    public class MetaPackage
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public Instant Published { get; set; }

        public int Downloads { get; set; }

        public Guid OwnerId { get; set; }

        public string Owner { get; set; }

        public MetaPackagePermission[] Permissions { get; set; }
    }
}