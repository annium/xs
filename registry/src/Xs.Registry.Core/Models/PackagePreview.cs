using System;
using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Core.Models
{
    public class PackagePreview
    {
        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public DateTime Published { get; }

        public uint Downloads { get; }

        public string Owner { get; }

        public IReadOnlyDictionary<PermissionCategory, Permission> Permissions { get; }

        public PackagePreview(IPackage package, MetaPackage metaPackage, User owner)
        {
            Name = package.Name;
            Version = package.Version;
            Description = package.Description;
            Published = package.Published;
            Downloads = package.Downloads;
            Owner = owner.Name;
            Permissions = metaPackage.Permissions;
        }
    }
}