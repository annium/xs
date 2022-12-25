using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;

namespace Xs.Commands.Sync;

internal class SyncAddCommand : AsyncCommand<SyncAddCommandConfiguration>
{
    public override string Id => "add";
    public override string Description => "Add repository to sync config";

    public override Task HandleAsync(
        SyncAddCommandConfiguration config,
        CancellationToken ct
    )
    {
        var paths = SyncConfig.Read();
        paths.Add(Path.GetFullPath(config.Path));
        SyncConfig.Write(paths);

        return Task.CompletedTask;
    }
}

internal class SyncAddCommandConfiguration
{
    [Position(1)]
    [Help("Repository path.")]
    public string Path { get; set; } = string.Empty;
}