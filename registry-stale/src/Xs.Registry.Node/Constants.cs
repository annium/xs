using Xs.Core.Models;

namespace Xs.Registry.Node
{
    internal static class Constants
    {
        public static readonly ProjectType ProjectType;

        static Constants()
        {
            var type = "node";
            ProjectType.Register(type);
            ProjectType = ProjectType.Get(type);
        }
    }
}