using Xs.Registry.Db.Shared;

namespace Xs.Registry.Dotnet
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