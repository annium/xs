using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.RegistryClient.Main;
using Xs.Tools;

namespace Xs.Commands.Remote;

internal class SetCommand : AsyncCommand<SetCommandConfiguration, DiscoverConfiguration>
{
    public override string Id => "set";
    public override string Description => "Start tracking registry.";
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IConfigurationManager _configurationManager;
    private readonly MainClientFactory _mainClientFactory;

    public SetCommand(
        DiscoverProjectsTask discoverTask,
        IConfigurationManager configurationManager,
        MainClientFactory mainClientFactory
    )
    {
        _discoverTask = discoverTask;
        _configurationManager = configurationManager;
        _mainClientFactory = mainClientFactory;
    }

    public override async Task HandleAsync(
        SetCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var location = cfg.Registry;
        var user = cfg.User;
        var dir = discoverCfg.Root;

        var client = _mainClientFactory.Create(location);

        var password = string.IsNullOrWhiteSpace(cfg.Password)
            ? Annium.Extensions.CommandLine.Cli.ReadSecure("Password: ")
            : cfg.Password;

        var configuration = _configurationManager.Load(dir);
        configuration.SetRegistry(location);
        configuration.SetToken(await client.LoginAsync(user, password));
        configuration.SetServers(
            (await client.GetRegistryInfoAsync())
            .Servers
            .ToDictionary(e => ProjectType.Get(e.Key), e => e.Value)
        );

        var projects = _discoverTask.RunAsync(discoverCfg).Await().ToArray();

        _configurationManager.Save(configuration, projects);

        Console.WriteLine("Registry tracking started");
    }
}

internal class SetCommandConfiguration
{
    [Position(1)]
    [Help("Registry location.")]
    public Uri Registry { get; set; } = new("http://localhost");

    [Option(isRequired: true)]
    [Help("User name.")]
    public string User { get; set; } = string.Empty;

    [Option(isRequired: false)]
    [Help("User password.")]
    public string Password { get; set; } = string.Empty;
}