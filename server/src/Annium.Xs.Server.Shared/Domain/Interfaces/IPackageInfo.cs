using NodaTime;

namespace Annium.Xs.Server.Shared.Domain.Interfaces;

public interface IPackageInfo
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    Instant Published { get; }
}
