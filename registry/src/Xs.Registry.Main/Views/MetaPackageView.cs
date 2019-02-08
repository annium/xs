using System;
using System.Collections.Generic;
using NodaTime;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Main.Views
{
    internal class MetaPackageView
    {
        public Guid Id { get; }

        public string Type { get; }

        public string Name { get; }

        public string Version { get; }

        public string Description { get; }

        public Instant Published { get; }

        public int Downloads { get; }

        public string Owner { get; }

        public IEnumerable<MetaPackagePermission> Permissions { get; }

        internal MetaPackageView(MetaPackage metaPackage)
        {
            Id = metaPackage.Id;
            Type = metaPackage.Type.ToString();
            Name = metaPackage.Name;
            Version = metaPackage.Version;
            Description = metaPackage.Description;
            Published = metaPackage.Published;
            Downloads = metaPackage.Downloads;
            Owner = metaPackage.Owner?.Name ?? string.Empty;
            Permissions = metaPackage.Permissions;
        }
    }
}