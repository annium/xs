using Xs.Cli.Core.Models;

namespace Xs.Cli.Dotnet
{
    internal static class Constants
    {
        public static readonly ProjectType ProjectType;

        static Constants()
        {
            var type = "dotnet";
            ProjectType.Register(type);
            ProjectType = ProjectType.Get(type);
        }
    }
}