using System;
using System.Collections.Generic;

namespace Xs.Cli.Dotnet.Models
{
    internal class TargetFramework
    {
        private static readonly IDictionary<string, TargetFramework> values = new Dictionary<string, TargetFramework>();

        public static TargetFramework NetStandard2_0 { get; } = new TargetFramework("netstandard2.0");
        public static TargetFramework NetStandard2_1 { get; } = new TargetFramework("netstandard2.1");
        public static TargetFramework NetStandard2_2 { get; } = new TargetFramework("netstandard2.2");
        public static TargetFramework NetCoreApp2_0 { get; } = new TargetFramework("netcoreapp2.0");
        public static TargetFramework NetCoreApp2_1 { get; } = new TargetFramework("netcoreapp2.1");
        public static TargetFramework NetCoreApp2_2 { get; } = new TargetFramework("netcoreapp2.2");
        private readonly string moniker;

        private TargetFramework(string moniker)
        {
            this.moniker = moniker;
            values[moniker] = this;
        }

        public override string ToString() => moniker;

        public static implicit operator TargetFramework(string value)
        {
            if (values.ContainsKey(value))
                return values[value];

            throw new ArgumentException($"Given value '{value}' is not a supported ({nameof(TargetFramework)}) moniker.");
        }
    }
}