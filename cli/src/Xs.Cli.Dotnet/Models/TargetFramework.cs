using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Xs.Cli.Dotnet.Models
{
    internal class TargetFramework : Equatable<TargetFramework>
    {
        private static readonly IDictionary<string, TargetFramework> values = new Dictionary<string, TargetFramework>();

        public static TargetFramework NetStandard2_0 { get; } = new TargetFramework("netstandard2.0");
        public static TargetFramework NetStandard2_1 { get; } = new TargetFramework("netstandard2.1");
        public static TargetFramework NetCoreApp2_0 { get; } = new TargetFramework("netcoreapp2.0");
        public static TargetFramework NetCoreApp2_1 { get; } = new TargetFramework("netcoreapp2.1");
        public static TargetFramework NetCoreApp2_2 { get; } = new TargetFramework("netcoreapp2.2");
        public static TargetFramework NetCoreApp3_0 { get; } = new TargetFramework("netcoreapp3.0");

        public static IReadOnlyList<IReadOnlyList<TargetFramework>> SupportedGroups { get; } =
        new List<IReadOnlyList<TargetFramework>>()
        {
            new List<TargetFramework> { NetStandard2_0, NetCoreApp2_0, NetCoreApp2_1, NetCoreApp2_2 },
            new List<TargetFramework> { NetStandard2_0, NetStandard2_1, NetCoreApp3_0 },
        };

        private readonly string moniker;

        private TargetFramework(string moniker)
        {
            this.moniker = moniker;
            values[moniker] = this;
        }

        public override string ToString() => moniker;

        public override IEnumerable<int> GetComponentHashCodes()
        {
            yield return moniker.GetHashCode();
        }

        public static implicit operator TargetFramework(string value)
        {
            if (values.ContainsKey(value))
                return values[value];

            throw new ArgumentException($"Given value '{value}' is not a supported ({nameof(TargetFramework)}) moniker.");
        }
    }
}