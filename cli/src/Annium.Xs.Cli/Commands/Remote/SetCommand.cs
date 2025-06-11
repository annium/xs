using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Attributes;
using Annium.Extensions.Arguments.Commands;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Server.Client;

namespace Annium.Xs.Cli.Commands.Remote;

internal class SetCommand : AsyncCommand<SetCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "set";
    public static string Description => "Start tracking registry.";
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
            ? Extensions.CommandLine.Cli.ReadSecure("Password: ")
            : cfg.Password;

        var configuration = _configurationManager.Load(dir);
        configuration.SetRegistry(location);
        var token = await client.LoginAsync(user, password);
        configuration.SetToken(token);
        var registryInfo = await client.GetRegistryInfoAsync();
        var servers = registryInfo.Servers.ToDictionary(e => e.Key.ParseEnum<ProjectType>(), e => e.Value);
        configuration.SetServers(servers);

        var projects = await _discoverTask.RunAsync(discoverCfg);

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
