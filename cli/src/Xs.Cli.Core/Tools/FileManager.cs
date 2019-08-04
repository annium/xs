using System;
using System.IO;
using System.Linq;

namespace Xs.Cli.Core.Tools
{
    public static class FileManager
    {
        public const string IgnoreFile = ".xs.ignore";
        private static readonly string[] globallyIgnoredDirectories = new [] { ".git" };

        public static bool FindDirectory(
            string directory,
            Func<string, bool> isMatch,
            string[] ignoredDirectories,
            bool checkSelf = false
        )
        {
            if (checkSelf)
            {
                if (IsDirectoryIgnored(directory, ignoredDirectories))
                    return false;
                if (isMatch(directory))
                    return true;
            }

            foreach (var child in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                if (FindDirectory(child, isMatch, ignoredDirectories, true))
                    return true;

            return false;
        }

        public static void WalkDirectories(
            string directory,
            Func<string, bool> isMatch,
            SearchOptions searchOptions,
            params string[] ignoredDirectories
        )
        {
            if (IsDirectoryIgnored(directory, ignoredDirectories))
                return;

            if (isMatch(directory))
            {
                if (searchOptions.HasFlag(SearchOptions.IgnoreChildrenOnMatch))
                    return;
            }

            foreach (var child in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                WalkDirectories(child, isMatch, searchOptions, ignoredDirectories);
        }

        public static bool IsRootedDirectoryIgnored(string root, string directory, string[] ignoredDirectories)
        {
            root = Path.GetFullPath(root);
            var dir = Path.GetFullPath(directory);

            do
            {
                if (IsDirectoryIgnored(dir, ignoredDirectories))
                    return true;

                dir = Directory.GetParent(dir)?.FullName;
            } while (dir != null && dir.Contains(root));

            return false;
        }

        public static bool IsUnrootedDirectoryIgnored(string directory, string[] ignoredDirectories)
        {
            var dir = Path.GetFullPath(directory);

            do
            {
                if (IsDirectoryIgnored(dir, ignoredDirectories))
                    return true;

                dir = Directory.GetParent(dir)?.FullName;
            } while (dir != null);

            return false;
        }

        private static bool IsDirectoryIgnored(string directory, string[] ignoredDirectories)
        {
            return globallyIgnoredDirectories.Any(directory.Contains) ||
                ignoredDirectories.Any(directory.Contains) ||
                !Directory.Exists(directory) ||
                Directory.GetFiles(directory, IgnoreFile, SearchOption.TopDirectoryOnly).Length > 0;
        }
    }

    [Flags]
    public enum SearchOptions
    {
        None = 0,

        IgnoreChildrenOnMatch = 1,
    }
}