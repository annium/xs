using System;
using System.IO;

namespace Xs.Registry.Node
{
    public class Configuration
    {
        public const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

        public readonly string PackagesFolder = Path.Combine("data", Constants.ProjectType.ToString(), "packages");

        public DatabaseConfiguration Database { get; set; }

        public Uri Location { get; set; }

        public Uri SharedLocation { get; set; }
    }

    public class DatabaseConfiguration
    {
        public string Name { get; set; }
    }
}