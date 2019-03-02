using System;
using System.Collections.Generic;
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
                    Console.WriteLine($"walk: {directory} - ignore");
                    return false;
                }

                Console.WriteLine($"walk: {directory} - check?");
                if (isMatch(directory))
                {
                    Console.WriteLine($"walk: {directory} - found");
                    return true;
                }
                else
                    Console.WriteLine($"walk: {directory} - omit");
            }

            foreach (var child in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                if (FindDirectory(child, isMatch, ignoredDirectories, true))
                    return true;

            return false;
        }

        public static List<string> CollectDirectories(
            string directory,
            Func<string, bool> isCollected,
            SearchOptions searchOptions = SearchOptions.None,
            params string[] ignoredDirectories
        )
        {
            var directories = new List<string>();

            CollectDirectories(directory, directories, isCollected, searchOptions, ignoredDirectories);

            return directories;
        }

        private static void CollectDirectories(
            string directory,
            IList<string> directories,
            Func<string, bool> isCollected,
            SearchOptions searchOptions,
            string[] ignoredDirectories
        )
        {
            if (IsDirectoryIgnored(directory, ignoredDirectories))
            {
                Console.WriteLine($"collect: {directory} - ignore");
                return;
            }

            Console.WriteLine($"collect: {directory} - check?");
            if (isCollected(directory))
            {
                Console.WriteLine($"collect: {directory} - collect");
                directories.Add(directory);

                if (searchOptions.HasFlag(SearchOptions.IgnoreChildrenOnMatch))
                    return;
            }
            else
                Console.WriteLine($"collect: {directory} - omit");

            foreach (var child in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                CollectDirectories(child, directories, isCollected, searchOptions, ignoredDirectories);
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