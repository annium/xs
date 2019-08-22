using Xs.Cli.Core.Models;

namespace Xs.Cli.Core
{
    internal static class Constants
    {
        public static readonly ProjectType MockProjectType;

        internal const string MockProjectTypeString = "mock";

        static Constants()
        {
            ProjectType.Register(MockProjectTypeString);
            MockProjectType = ProjectType.Get(MockProjectTypeString);
        }
    }
}