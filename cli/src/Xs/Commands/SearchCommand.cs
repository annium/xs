using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Server.Client;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;

namespace Xs.Commands;

internal class SearchCommand
    : AsyncCommand<SearchCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "search";
    public static string Description => "Search for packages in tracked registry.";
    public ILogger Logger { get; }
    private readonly IConfigurationManager _configurationManager;
    private readonly MainClientFactory _mainClientFactory;

    public SearchCommand(
        IConfigurationManager configurationManager,
        MainClientFactory mainClientFactory,
        ILogger logger
    )
    {
        _configurationManager = configurationManager;
        _mainClientFactory = mainClientFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(
        SearchCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var configuration = _configurationManager.Load(discoverCfg.Root);

        var client = _mainClientFactory.Create(configuration.Registry);
        var packages = await client.SearchAsync(configuration.Token, cfg.Type.ToString(), cfg.Query);

        foreach (var package in packages)
            Console.WriteLine($"{package.Name}: {package.Version} ({package.Description})");
    }
}

internal class SearchCommandConfiguration
{
    [Position(1)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Position(2)]
    [Help("Search query.")]
    public string Query { get; set; } = string.Empty;
}
