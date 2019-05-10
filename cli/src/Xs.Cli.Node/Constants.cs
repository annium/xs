using Xs.Cli.Core.Models;

namespace Xs.Cli.Node
{
    internal static class Constants
    {
        public const string DefaultServer = "https://registry.npmjs.com";

        public static readonly ProjectType ProjectType;

        static Constants()
        {
            var type = "node";
            ProjectType.Register(type);
            ProjectType = ProjectType.Get(type);
        }
    }
}