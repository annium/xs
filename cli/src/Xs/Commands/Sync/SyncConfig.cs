using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xs.Commands.Sync;

public static class SyncConfig
{
    private static readonly string ConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".xs.sync");

    public static List<string> Read()
    {
        return !File.Exists(ConfigFile) ? new() : File.ReadAllLines(ConfigFile).ToList();
    }

    public static void Write(IReadOnlyCollection<string> config)
    {
        var paths = config.Select(Path.GetFullPath).OrderBy(x => x).ToHashSet();
        File.WriteAllLines(ConfigFile, paths);
    }
}