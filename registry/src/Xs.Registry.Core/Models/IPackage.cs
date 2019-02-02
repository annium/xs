using System;

namespace Xs.Registry.Core.Models
{
    public interface IPackage
    {
        string Id { get; }

        string MetaPackageId { get; }

        string Name { get; }

        string Version { get; }

        string Description { get; }

        DateTime Published { get; }

        uint Downloads { get; set; }
    }
}