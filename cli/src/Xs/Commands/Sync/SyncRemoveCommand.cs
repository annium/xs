using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;

namespace Xs.Commands.Sync;

internal class SyncRemoveCommand : AsyncCommand<SyncRemoveCommandConfiguration>
{
    public override string Id => "remove";
    public override string Description => "Remove repository from sync config";

    public override Task HandleAsync(
        SyncRemoveCommandConfiguration config,
        CancellationToken ct
    )
    {
        var paths = SyncConfig.Read();
        paths.Remove(Path.GetFullPath(config.Path));
        SyncConfig.Write(paths);

        return Task.CompletedTask;
    }
}

internal class SyncRemoveCommandConfiguration
{
    [Position(1)]
    [Help("Repository path.")]
    public string Path { get; set; } = string.Empty;
}