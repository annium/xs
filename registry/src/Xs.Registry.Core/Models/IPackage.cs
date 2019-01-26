using System;

namespace Xs.Registry.Core.Models
{
    public interface IPackage
    {
        string Name { get; }

        string Version { get; }

        string Description { get; }

        DateTime Published { get; }
    }
}