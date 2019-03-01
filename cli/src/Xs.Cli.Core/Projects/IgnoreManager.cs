using System.IO;

namespace Xs.Cli.Core.Projects
{
    public static class IgnoreManager
    {
        public const string IgnoreMarker = ".xs.ignore";

        public static bool IsDirectoryIgnored(string directory, bool recursively = false)
        {
            if (!recursively)
                return IsIgnored();

            do
            {
                if (IsIgnored())
                    return true;

                directory = Directory.GetParent(directory)?.FullName;
            }
            while (directory != null);

            return false;

            bool IsIgnored() => Directory.GetFiles(directory, IgnoreMarker, SearchOption.TopDirectoryOnly).Length > 0;
        }
    }
}