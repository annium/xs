using System;
using System.Collections.Generic;

namespace Xs.Cli.Dotnet.Models;

internal class TargetFramework
{
    private static readonly IDictionary<string, TargetFramework> Values = new Dictionary<string, TargetFramework>();

    public static TargetFramework NetStandard20 { get; } = new("netstandard2.0");
    public static TargetFramework NetStandard21 { get; } = new("netstandard2.1");
    public static TargetFramework NetCoreApp20 { get; } = new("netcoreapp2.0");
    public static TargetFramework NetCoreApp21 { get; } = new("netcoreapp2.1");
    public static TargetFramework NetCoreApp22 { get; } = new("netcoreapp2.2");
    public static TargetFramework NetCoreApp30 { get; } = new("netcoreapp3.0");
    public static TargetFramework NetCoreApp31 { get; } = new("netcoreapp3.1");
    public static TargetFramework Net5 { get; } = new("net5.0");
    public static TargetFramework Net6 { get; } = new("net6.0");
    public static TargetFramework Net6Android { get; } = new("net6.0-android");
    public static TargetFramework Net6Ios { get; } = new("net6.0-ios");
    public static TargetFramework Net6MacCatalyst { get; } = new("net6.0-maccatalyst");
    public static TargetFramework Net6MacOs { get; } = new("net6.0-macos");
    public static TargetFramework Net6TvOs { get; } = new("net6.0-tvos");
    public static TargetFramework Net6Windows { get; } = new("net6.0-windows");
    public static TargetFramework Net7 { get; } = new("net7.0");
    public static TargetFramework Net7Android { get; } = new("net7.0-android");
    public static TargetFramework Net7Ios { get; } = new("net7.0-ios");
    public static TargetFramework Net7MacCatalyst { get; } = new("net7.0-maccatalyst");
    public static TargetFramework Net7MacOs { get; } = new("net7.0-macos");
    public static TargetFramework Net7TvOs { get; } = new("net7.0-tvos");
    public static TargetFramework Net7Windows { get; } = new("net7.0-windows");

    public static IReadOnlyList<IReadOnlyList<TargetFramework>> SupportedGroups { get; } =
        new List<IReadOnlyList<TargetFramework>>()
        {
            new List<TargetFramework> { NetStandard20, NetCoreApp20, NetCoreApp21, NetCoreApp22 },
            new List<TargetFramework> { NetStandard20, NetStandard21, NetCoreApp30 },
            new List<TargetFramework> { NetStandard21, NetCoreApp31 },
            new List<TargetFramework> { NetStandard21, Net5 },
            new List<TargetFramework> { NetStandard21, Net6, Net6Android, Net6Ios, Net6MacCatalyst, Net6MacOs, Net6TvOs, Net6Windows },
            new List<TargetFramework> { NetStandard21, Net7, Net7Android, Net7Ios, Net7MacCatalyst, Net7MacOs, Net7TvOs, Net7Windows },
        };

    private readonly string _moniker;

    private TargetFramework(string moniker)
    {
        _moniker = moniker;
        Values[moniker] = this;
    }

    public override string ToString() => _moniker;

    public override int GetHashCode() => _moniker.GetHashCode();

    public static implicit operator TargetFramework(string value)
    {
        if (Values.TryGetValue(value, out var tfm))
            return tfm;

        throw new ArgumentException($"Given value '{value}' is not a supported ({nameof(TargetFramework)}) moniker.");
    }
}