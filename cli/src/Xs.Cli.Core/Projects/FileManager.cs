using System.IO;
using System.Linq;

namespace Xs.Cli.Core.Projects
{
    public static class FileManager
    {
        public const string IgnoreFile = ".xs.ignore";

        private static readonly string[] ignoredDirectories = new [] { ".git" };

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

            bool IsIgnored() => ignoredDirectories.Any(directory.Contains) ||
                Directory.GetFiles(directory, IgnoreFile, SearchOption.TopDirectoryOnly).Length > 0;
        }
    }
}