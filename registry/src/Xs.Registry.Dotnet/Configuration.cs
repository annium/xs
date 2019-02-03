using System;
using System.IO;

namespace Xs.Registry.Dotnet
{
    public class Configuration
    {
        public readonly string PackagesFolder = Path.Combine("data", Constants.ProjectType.ToString(), "packages");

        public Uri Location { get; set; }

        public Uri SharedLocation { get; set; }
    }
}