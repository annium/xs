using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;

namespace Xs.Commands.Sync;

internal class SyncListCommand : AsyncCommand
{
    public override string Id => "list";
    public override string Description => "List repositories for sync";

    public override Task HandleAsync(
        CancellationToken ct
    )
    {
        var paths = SyncConfig.Read();
        foreach (var path in paths)
            Console.WriteLine(path);

        return Task.CompletedTask;
    }
}