using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Shell;

namespace Xs.Cli.Core.Tasks
{
    public class DiscoverChangedFilesTask
    {
        private readonly IShell _shell;

        public DiscoverChangedFilesTask(
            IShell shell
        )
        {
            _shell = shell;
        }

        public async Task<IReadOnlyCollection<string>> RunAsync(IReadOnlyCollection<string> roots)
        {
            var changes = await Task.WhenAll(roots.Select(LoadChangesAsync));

            return changes.SelectMany(x => x).Distinct().ToHashSet();
        }

        private async Task<IReadOnlyCollection<string>> LoadChangesAsync(string root)
        {
            var result = await _shell.Cmd("git st -s")
                .Configure(new ProcessStartInfo { WorkingDirectory = root })
                .RunAsync();

            if (!result.IsSuccess)
                throw new Exception($"Failed to get repo status at {root}");

            var files = result.Output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .SelectMany(ParseLine)
                .ToArray();

            return files;

            IEnumerable<string> ParseLine(string line)
            {
                // path is specified after modificator:
                // R  .xs -> .xss
                // M  cli/src/Xs.Cli.Core/Commands/DiscoverConfiguration.cs
                var path = line.Split(" ", 2)[1].Trim();
                // if file was moved - 2 paths in single line
                if (path.Contains("->"))
                    foreach (var chunk in path.Split("->"))
                        yield return Path.GetFullPath(chunk.Trim(), root);
                else
                    yield return Path.GetFullPath(path, root);
            }
        }
    }
}