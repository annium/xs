using System;
using System.Collections.Generic;

namespace Annium.Xs.Cli.Dotnet.Models;

internal class TargetFramework
{
    private static readonly IDictionary<string, TargetFramework> _values = new Dictionary<string, TargetFramework>();

    public static TargetFramework NetStandard20 { get; } = new("netstandard2.0");
    public static TargetFramework NetStandard21 { get; } = new("netstandard2.1");
    public static TargetFramework NetCoreApp20 { get; } = new("netcoreapp2.0");
    public static TargetFramework NetCoreApp21 { get; } = new("netcoreapp2.1");
    public static TargetFramework NetCoreApp22 { get; } = new("netcoreapp2.2");
    public static TargetFramework NetCoreApp30 { get; } = new("netcoreapp3.0");
    public static TargetFramework NetCoreApp31 { get; } = new("netcoreapp3.1");
    public static TargetFramework Net5_0 { get; } = new("net5.0");
    public static TargetFramework Net5 { get; } = new("net5");
    public static TargetFramework Net6_0 { get; } = new("net6.0");
    public static TargetFramework Net6 { get; } = new("net6");
    public static TargetFramework Net6_0Android { get; } = new("net6.0-android");
    public static TargetFramework Net6Android { get; } = new("net6-android");
    public static TargetFramework Net6_0Ios { get; } = new("net6.0-ios");
    public static TargetFramework Net6Ios { get; } = new("net6-ios");
    public static TargetFramework Net6_0MacCatalyst { get; } = new("net6.0-maccatalyst");
    public static TargetFramework Net6MacCatalyst { get; } = new("net6-maccatalyst");
    public static TargetFramework Net6_0MacOs { get; } = new("net6.0-macos");
    public static TargetFramework Net6MacOs { get; } = new("net6-macos");
    public static TargetFramework Net6_0TvOs { get; } = new("net6.0-tvos");
    public static TargetFramework Net6TvOs { get; } = new("net6-tvos");
    public static TargetFramework Net6_0Windows { get; } = new("net6.0-windows");
    public static TargetFramework Net6Windows { get; } = new("net6-windows");
    public static TargetFramework Net7_0 { get; } = new("net7.0");
    public static TargetFramework Net7 { get; } = new("net7");
    public static TargetFramework Net7_0Android { get; } = new("net7.0-android");
    public static TargetFramework Net7Android { get; } = new("net7-android");
    public static TargetFramework Net7_0Ios { get; } = new("net7.0-ios");
    public static TargetFramework Net7Ios { get; } = new("net7-ios");
    public static TargetFramework Net7_0MacCatalyst { get; } = new("net7.0-maccatalyst");
    public static TargetFramework Net7MacCatalyst { get; } = new("net7-maccatalyst");
    public static TargetFramework Net7_0MacOs { get; } = new("net7.0-macos");
    public static TargetFramework Net7MacOs { get; } = new("net7-macos");
    public static TargetFramework Net7_0TvOs { get; } = new("net7.0-tvos");
    public static TargetFramework Net7TvOs { get; } = new("net7-tvos");
    public static TargetFramework Net7_0Windows { get; } = new("net7.0-windows");
    public static TargetFramework Net7Windows { get; } = new("net7-windows");
    public static TargetFramework Net8_0 { get; } = new("net8.0");
    public static TargetFramework Net8 { get; } = new("net8");
    public static TargetFramework Net8_0Android { get; } = new("net8.0-android");
    public static TargetFramework Net8Android { get; } = new("net8-android");
    public static TargetFramework Net8_0Ios { get; } = new("net8.0-ios");
    public static TargetFramework Net8Ios { get; } = new("net8-ios");
    public static TargetFramework Net8_0MacCatalyst { get; } = new("net8.0-maccatalyst");
    public static TargetFramework Net8MacCatalyst { get; } = new("net8-maccatalyst");
    public static TargetFramework Net8_0MacOs { get; } = new("net8.0-macos");
    public static TargetFramework Net8MacOs { get; } = new("net8-macos");
    public static TargetFramework Net8_0TvOs { get; } = new("net8.0-tvos");
    public static TargetFramework Net8TvOs { get; } = new("net8-tvos");
    public static TargetFramework Net8_0Windows { get; } = new("net8.0-windows");
    public static TargetFramework Net8Windows { get; } = new("net8-windows");
    public static TargetFramework Net9_0 { get; } = new("net9.0");
    public static TargetFramework Net9 { get; } = new("net9");
    public static TargetFramework Net9_0Android { get; } = new("net9.0-android");
    public static TargetFramework Net9Android { get; } = new("net9-android");
    public static TargetFramework Net9_0Ios { get; } = new("net9.0-ios");
    public static TargetFramework Net9Ios { get; } = new("net9-ios");
    public static TargetFramework Net9_0MacCatalyst { get; } = new("net9.0-maccatalyst");
    public static TargetFramework Net9MacCatalyst { get; } = new("net9-maccatalyst");
    public static TargetFramework Net9_0MacOs { get; } = new("net9.0-macos");
    public static TargetFramework Net9MacOs { get; } = new("net9-macos");
    public static TargetFramework Net9_0TvOs { get; } = new("net9.0-tvos");
    public static TargetFramework Net9TvOs { get; } = new("net9-tvos");
    public static TargetFramework Net9_0Windows { get; } = new("net9.0-windows");
    public static TargetFramework Net9Windows { get; } = new("net9-windows");
    public static TargetFramework Net10_0 { get; } = new("net10.0");
    public static TargetFramework Net10 { get; } = new("net10");
    public static TargetFramework Net10_0Android { get; } = new("net10.0-android");
    public static TargetFramework Net10Android { get; } = new("net10-android");
    public static TargetFramework Net10_0Ios { get; } = new("net10.0-ios");
    public static TargetFramework Net10Ios { get; } = new("net10-ios");
    public static TargetFramework Net10_0MacCatalyst { get; } = new("net10.0-maccatalyst");
    public static TargetFramework Net10MacCatalyst { get; } = new("net10-maccatalyst");
    public static TargetFramework Net10_0MacOs { get; } = new("net10.0-macos");
    public static TargetFramework Net10MacOs { get; } = new("net10-macos");
    public static TargetFramework Net10_0TvOs { get; } = new("net10.0-tvos");
    public static TargetFramework Net10TvOs { get; } = new("net10-tvos");
    public static TargetFramework Net10_0Windows { get; } = new("net10.0-windows");
    public static TargetFramework Net10Windows { get; } = new("net10-windows");

    public static IReadOnlyList<IReadOnlyList<TargetFramework>> SupportedGroups { get; } =
        new List<IReadOnlyList<TargetFramework>>
        {
            new List<TargetFramework> { NetStandard20, NetCoreApp20, NetCoreApp21, NetCoreApp22 },
            new List<TargetFramework> { NetStandard20, NetStandard21, NetCoreApp30 },
            new List<TargetFramework> { NetStandard21, NetCoreApp31 },
            new List<TargetFramework> { NetStandard21, Net5, Net5_0 },
            new List<TargetFramework>
            {
                NetStandard21,
                Net6,
                Net6_0,
                Net6Android,
                Net6_0Android,
                Net6Ios,
                Net6_0Ios,
                Net6MacCatalyst,
                Net6_0MacCatalyst,
                Net6MacOs,
                Net6_0MacOs,
                Net6TvOs,
                Net6_0TvOs,
                Net6Windows,
                Net6_0Windows,
            },
            new List<TargetFramework>
            {
                NetStandard21,
                Net7,
                Net7_0,
                Net7Android,
                Net7_0Android,
                Net7Ios,
                Net7_0Ios,
                Net7MacCatalyst,
                Net7_0MacCatalyst,
                Net7MacOs,
                Net7_0MacOs,
                Net7TvOs,
                Net7_0TvOs,
                Net7Windows,
                Net7_0Windows,
            },
            new List<TargetFramework>
            {
                NetStandard21,
                Net8,
                Net8_0,
                Net8Android,
                Net8_0Android,
                Net8Ios,
                Net8_0Ios,
                Net8MacCatalyst,
                Net8_0MacCatalyst,
                Net8MacOs,
                Net8_0MacOs,
                Net8TvOs,
                Net8_0TvOs,
                Net8Windows,
                Net8_0Windows,
            },
            new List<TargetFramework>
            {
                NetStandard21,
                Net9,
                Net9_0,
                Net9Android,
                Net9_0Android,
                Net9Ios,
                Net9_0Ios,
                Net9MacCatalyst,
                Net9_0MacCatalyst,
                Net9MacOs,
                Net9_0MacOs,
                Net9TvOs,
                Net9_0TvOs,
                Net9Windows,
                Net9_0Windows,
            },
            new List<TargetFramework>
            {
                NetStandard21,
                Net10,
                Net10_0,
                Net10Android,
                Net10_0Android,
                Net10Ios,
                Net10_0Ios,
                Net10MacCatalyst,
                Net10_0MacCatalyst,
                Net10MacOs,
                Net10_0MacOs,
                Net10TvOs,
                Net10_0TvOs,
                Net10Windows,
                Net10_0Windows,
            },
        };

    private readonly string _moniker;

    private TargetFramework(string moniker)
    {
        _moniker = moniker;
        _values[moniker] = this;
    }

    public override string ToString() => _moniker;

    public override int GetHashCode() => _moniker.GetHashCode();

    public static implicit operator TargetFramework(string value)
    {
        if (_values.TryGetValue(value, out var tfm))
            return tfm;

        throw new ArgumentException($"Given value '{value}' is not a supported ({nameof(TargetFramework)}) moniker.");
    }
}
