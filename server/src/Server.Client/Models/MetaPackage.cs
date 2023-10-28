using System;
using NodaTime;

namespace Server.Client.Models;

public class MetaPackage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Instant Published { get; init; }
    public int Downloads { get; init; }
    public Guid OwnerId { get; init; }
    public string Owner { get; init; } = string.Empty;
    public MetaPackagePermission[] Permissions { get; init; } = Array.Empty<MetaPackagePermission>();
}
