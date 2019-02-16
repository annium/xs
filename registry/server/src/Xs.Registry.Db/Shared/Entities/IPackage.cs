using System;
using System.Collections.Generic;

namespace Xs.Registry.Db.Shared.Entities
{
    public interface IPackage<TDependency>
    {
        Guid Id { get; set; }

        string LowerName { get; }

        string Version { get; }

        int Downloads { get; set; }

        List<TDependency> Dependencies { get; set; }
    }
}