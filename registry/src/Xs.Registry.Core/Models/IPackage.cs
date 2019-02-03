using System;

namespace Xs.Registry.Core.Models
{
    public interface IPackage : IPackageBase
    {
        Guid Id { get; }

        Guid MetaPackageId { get; }

        // MetaPackage MetaPackage { get; }

        uint Downloads { get; }
    }
}