using System;
using System.Linq;
using System.Threading;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;

namespace Xs.Commands.Remote;

internal class SetLocalCommand : Command<SetLocalCommandConfiguration, DiscoverConfiguration>
{
    public override string Id => "set-local";
    public override string Description => "Set local registry.";
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

    public override void Handle(
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
        configuration.SetServers(ProjectType.List().ToDictionary(type => type, type => location));

        var projects = _discoverTask.RunAsync(discoverCfg).Await().ToArray();

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