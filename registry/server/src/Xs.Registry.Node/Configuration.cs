using System;
using System.IO;
using Xs.Registry.Abstract;

namespace Xs.Registry.Node
{
    internal class Configuration : IConfiguration
    {
        public const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

        public readonly string PackagesFolder = Path.Combine("data", Constants.ProjectType.ToString(), "packages");

        public Uri Location { get; set; }

        public Uri MainLocation { get; set; }
    }
}