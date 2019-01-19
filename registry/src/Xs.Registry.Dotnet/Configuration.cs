using System;
using System.IO;

namespace Xs.Registry.Dotnet
{
    public class Configuration
    {
        public readonly string PackagesFolder = Path.Combine("data", "dotnet", "packages");

        public DatabaseConfiguration Database { get; set; }

        public Uri Location { get; set; }

        public Uri SharedLocation { get; set; }
    }

    public class DatabaseConfiguration
    {
        public string Name { get; set; }
    }
}