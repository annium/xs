using NodaTime;

namespace Server.Domain.Models;

public interface IPackageInfo
{
    string Name { get; }

    string Version { get; }

    string Description { get; }

    Instant Published { get; }
}