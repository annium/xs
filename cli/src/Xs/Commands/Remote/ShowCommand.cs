using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Core.Tools;

namespace Xs.Commands.Remote;

internal class ShowCommand : AsyncCommand<DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "show";
    public static string Description => "Show information about tracked registry.";
    private readonly IConfigurationManager _configurationManager;

    public ShowCommand(
        IConfigurationManager configurationManager
    )
    {
        _configurationManager = configurationManager;
    }

    public override async Task HandleAsync(
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var dir = discoverCfg.Root;

        var configuration = _configurationManager.Load(dir);

        Console.Write(Yaml.Serializer.Serialize(configuration));
    }
}