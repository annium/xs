using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Xs.Cli.Dotnet.Models;

internal class TargetFramework : Equatable<TargetFramework>
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

    public static IReadOnlyList<IReadOnlyList<TargetFramework>> SupportedGroups { get; } =
        new List<IReadOnlyList<TargetFramework>>()
        {
            new List<TargetFramework> { NetStandard20, NetCoreApp20, NetCoreApp21, NetCoreApp22 },
            new List<TargetFramework> { NetStandard20, NetStandard21, NetCoreApp30 },
            new List<TargetFramework> { NetStandard21, NetCoreApp31 },
            new List<TargetFramework> { NetStandard21, Net5 },
            new List<TargetFramework> { Net6 },
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
        if (Values.ContainsKey(value))
            return Values[value];

        throw new ArgumentException($"Given value '{value}' is not a supported ({nameof(TargetFramework)}) moniker.");
    }
}