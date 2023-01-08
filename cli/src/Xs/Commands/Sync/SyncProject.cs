namespace Xs.Commands.Sync;

public sealed record SyncProject
{
    public string Path { get; init; } = string.Empty;
    public SyncProjectConfig Config { get; init; } = new();
    public static SyncProject CreateDefault(string path) => new() { Path = path };
    public override string ToString() => $"{Path}: {Config}";
}

public sealed record SyncProjectConfig
{
    public bool Push { get; init; }

    public override string ToString() => $"push={Push}";
}