using System;
using System.Collections.Generic;
using NodaTime;

namespace Xs.Registry.Db.Shared.Entities
{
    public interface IPackage<TDependency>
    {
        Guid Id { get; set; }

        Guid MetaPackageId { get; }

        string Name { get; }

        string LowerName { get; }

        string Version { get; }

        string Description { get; }

        Instant Published { get; }

        int Downloads { get; set; }

        List<TDependency> Dependencies { get; set; }
    }
}