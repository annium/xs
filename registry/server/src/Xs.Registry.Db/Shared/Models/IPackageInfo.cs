using NodaTime;

namespace Xs.Registry.Db.Shared
{
    public interface IPackageInfo
    {
        string Name { get; }

        string Version { get; }

        string Description { get; }

        Instant Published { get; }
    }
}