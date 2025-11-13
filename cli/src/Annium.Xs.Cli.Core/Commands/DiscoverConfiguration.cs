using System.IO;
using System.Linq;
using Annium.Extensions.Arguments;

namespace Annium.Xs.Cli.Core.Commands;

public class DiscoverConfiguration
{
    [Option("cwd")]
    [Help("Allows to run command in specific folder.")]
    public string[] Roots
    {
        get => field.Length > 0 ? field : [Directory.GetCurrentDirectory()];
        set
        {
            var strings = value.Select(Path.GetFullPath).ToArray();
            field = strings;
        }
    } = [];

    [Option("c")]
    [Help("Filters only projects with changes in VCS.")]
    public bool Changed { get; set; }

    public string Root => Roots.First();

    [Option("sc")]
    [Help("Allows to disable normally forced checks.")]
    public bool SkipChecks { get; set; }

    [Option("fc")]
    [Help("Force string checks.")]
    public bool ForceChecks { get; set; }

    [Option("ic")]
    [Help("Allows to ignore inconsistency to fix fursther.")]
    public bool IgnoreConsistency { get; set; }
}
