using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;

namespace Xs.Commands.Remote;

internal class SetLocalCommand : AsyncCommand<SetLocalCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "set-local";
    public static string Description => "Set local registry.";
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IConfigurationManager _configurationManager;

    public SetLocalCommand(
        DiscoverProjectsTask discoverTask,
        IConfigurationManager configurationManager
    )
    {
        _discoverTask = discoverTask;
        _configurationManager = configurationManager;
    }

    public override async Task HandleAsync(
        SetLocalCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = cfg.Registry;
        var dir = discoverCfg.Root;

        var configuration = _configurationManager.Load(dir);
        configuration.SetRegistry(location);
        configuration.SetToken(string.Empty);
        configuration.SetServers(Enum.GetValues<ProjectType>().Except(ProjectType.None.Yield()).ToDictionary(type => type, _ => location));

        var projects = await _discoverTask.RunAsync(discoverCfg);

        _configurationManager.Save(configuration, projects);

        Console.WriteLine("Registry tracking started");
    }
}

internal class SetLocalCommandConfiguration
{
    [Position(1)]
    [Help("Registry location.")]
    public Uri Registry { get; set; } = new("http://localhost");
}