using NodaTime;

namespace Xs.Registry.Core.Models
{
    public interface IPackageBase
    {
        string Name { get; }

        string Version { get; }

        string Description { get; }

        Instant Published { get; }
    }
}