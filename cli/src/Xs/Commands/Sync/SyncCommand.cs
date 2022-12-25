using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;

namespace Xs.Commands.Sync;

internal class SyncCommand : AsyncCommand, ILogSubject<SyncCommand>
{
    public override string Id => "";
    public override string Description => "Execute repositories sync";
    public ILogger<SyncCommand> Logger { get; }

    public SyncCommand(
        ILogger<SyncCommand> logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(
        CancellationToken ct
    )
    {
        var paths = SyncConfig.Read();
        this.Log().Info("Sync {count} project(s) - start", paths.Count);
        await Task.CompletedTask;
        this.Log().Info("Sync {count} project(s) - done", paths.Count);
    }
}