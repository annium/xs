using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Commands;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Core.Tools;

namespace Annium.Xs.Cli.Commands.Remote;

internal class DeleteCommand : AsyncCommand<DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "delete";
    public static string Description => "Stop tracking registry.";
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IConfigurationManager _configurationManager;

    public DeleteCommand(DiscoverProjectsTask discoverTask, IConfigurationManager configurationManager)
    {
        _discoverTask = discoverTask;
        _configurationManager = configurationManager;
    }

    public override async Task HandleAsync(DiscoverConfiguration discoverCfg, CancellationToken ct)
    {
        var dir = discoverCfg.Root;

        var projects = await _discoverTask.RunAsync(discoverCfg);

        _configurationManager.Delete(dir, projects);

        Console.WriteLine("Registry tracking stopped.");
    }
}
