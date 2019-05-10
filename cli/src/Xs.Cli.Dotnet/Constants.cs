using Xs.Cli.Core.Models;

namespace Xs.Cli.Dotnet
{
    internal static class Constants
    {
        public const string DefaultServer = "https://api.nuget.org";

        public const string ServerPathSuffix = "/v3/index.json";

        public static readonly ProjectType ProjectType;

        static Constants()
        {
            var type = "dotnet";
            ProjectType.Register(type);
            ProjectType = ProjectType.Get(type);
        }
    }
}