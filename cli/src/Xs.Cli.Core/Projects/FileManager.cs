using System;
using System.IO;
using System.Linq;

namespace Xs.Cli.Core.Projects
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
                {
                    Console.WriteLine($"find: {directory} - ignore");
                    return false;
                }

                Console.WriteLine($"find: {directory} - check?");
                if (isMatch(directory))
                {
                    Console.WriteLine($"find: {directory} - found");
                    return true;
                }
                else
                    Console.WriteLine($"find: {directory} - omit");
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
            {
                Console.WriteLine($"walk: {directory} - ignore");
                return;
            }

            Console.WriteLine($"walk: {directory} - check?");
            if (isMatch(directory))
            {
                Console.WriteLine($"walk: {directory} - collect");
                if (searchOptions.HasFlag(SearchOptions.IgnoreChildrenOnMatch))
                    return;
            }
            else
                Console.WriteLine($"walk: {directory} - omit");

            foreach (var child in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                WalkDirectories(child, isMatch, searchOptions, ignoredDirectories);
        }

        public static bool IsRootedDirectoryIgnored(string root, string directory, string[] ignoredDirectories)
        {
            root = Path.GetFullPath(root);
            var dir = Path.GetFullPath(directory);

            do
            {
                Console.WriteLine($"ignore: {dir} - check?");
                if (IsDirectoryIgnored(dir, ignoredDirectories))
                {
                    Console.WriteLine($"ignore: {dir} - yes");
                    return true;
                }
                dir = Directory.GetParent(dir)?.FullName;
            } while (dir != null && dir.Contains(root));

            Console.WriteLine($"ignore: {directory} - false");

            return false;
        }

        public static bool IsUnrootedDirectoryIgnored(string directory, string[] ignoredDirectories)
        {
            var dir = Path.GetFullPath(directory);

            do
            {
                Console.WriteLine($"ignore: {dir} - check?");
                if (IsDirectoryIgnored(dir, ignoredDirectories))
                {
                    Console.WriteLine($"ignore: {dir} - yes");
                    return true;
                }
                dir = Directory.GetParent(dir)?.FullName;
            } while (dir != null);

            Console.WriteLine($"ignore: {directory} - false");

            return false;
        }

        private static bool IsDirectoryIgnored(string directory, string[] ignoredDirectories)
        {
            return globallyIgnoredDirectories.Any(directory.Contains) ||
                ignoredDirectories.Any(directory.Contains) ||
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