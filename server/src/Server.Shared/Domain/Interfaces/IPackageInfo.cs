using NodaTime;

namespace Server.Shared.Domain.Interfaces;

public interface IPackageInfo
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    Instant Published { get; }
}