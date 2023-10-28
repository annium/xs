namespace Xs.Commands.Sync;

public sealed record SyncProject
{
    public static SyncProject CreateDefault(string path) => new() { Path = path };

    public string Path { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public SyncProjectConfig Config { get; init; } = new();

    public override string ToString() => $"{Path} ({Group}): {Config}";
}

public sealed record SyncProjectConfig
{
    public bool Push { get; init; }

    public override string ToString() => $"push={Push}";
}
