using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Threading.Tasks;
using Server.Client;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;

namespace Xs.Commands.Remote;

internal class RestoreCommand : AsyncCommand<RestoreCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "restore";
    public static string Description => "Restore tracked registry information.";
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IConfigurationManager _configurationManager;
    private readonly MainClientFactory _mainClientFactory;

    public RestoreCommand(
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
        RestoreCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var user = cfg.User;
        var dir = discoverCfg.Root;

        var configuration = _configurationManager.Load(dir);
        if (configuration == Configuration.Empty)
        {
            Console.WriteLine("No configuration file, skip restore");
            return;
        }

        var client = _mainClientFactory.Create(configuration.Registry);

        var password = string.IsNullOrWhiteSpace(cfg.Password)
            ? Annium.Extensions.CommandLine.Cli.ReadSecure("Password: ")
            : cfg.Password;

        configuration.SetToken(await client.LoginAsync(user, password));
        configuration.SetServers(
            (await client.GetRegistryInfoAsync())
            .Servers
            .ToDictionary(e => ProjectType.Get(e.Key), e => e.Value)
        );

        var projects = _discoverTask.RunAsync(discoverCfg).Await().ToArray();

        _configurationManager.Save(configuration, projects);

        Console.WriteLine("Registry restored");
    }
}

internal class RestoreCommandConfiguration
{
    [Option(isRequired: true)]
    [Help("User name.")]
    public string User { get; set; } = string.Empty;

    [Option(isRequired: false)]
    [Help("User password.")]
    public string Password { get; set; } = string.Empty;
}