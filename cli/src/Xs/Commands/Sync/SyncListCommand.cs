using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;

namespace Xs.Commands.Sync;

internal class SyncListCommand : AsyncCommand
{
    public override string Id => "list";
    public override string Description => "List repositories for sync";
    private readonly SyncConfigurator _configurator;

    public SyncListCommand(
        SyncConfigurator configurator
    )
    {
        _configurator = configurator;
    }

    public override Task HandleAsync(
        CancellationToken ct
    )
    {
        var projects = _configurator.Read();
        foreach (var project in projects)
            Console.WriteLine(project.ToString());

        return Task.CompletedTask;
    }
}