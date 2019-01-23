using System;
using System.Collections.Generic;

namespace Xs.Cli.Dotnet.Models
{
    internal enum TargetFramework
    {
        NetStandard2_0,
        NetCoreApp2_0,
        NetCoreApp2_1,
        NetCoreApp2_2,
    }

    internal static class TargetFrameworkParser
    {
        private static readonly IDictionary<string, TargetFramework> values;

        static TargetFrameworkParser()
        {
            values = new Dictionary<string, TargetFramework>();
            values["netstandard2.0"] = TargetFramework.NetStandard2_0;
            values["netcoreapp2.0"] = TargetFramework.NetCoreApp2_0;
            values["netcoreapp2.1"] = TargetFramework.NetCoreApp2_1;
            values["netcoreapp2.2"] = TargetFramework.NetCoreApp2_2;
        }

        public static TargetFramework Parse(string value)
        {
            if (!values.ContainsKey(value))
                throw new ArgumentException($"Given value '{value}' is not a supported TargetFramework moniker.");

            return values[value];
        }
    }
}