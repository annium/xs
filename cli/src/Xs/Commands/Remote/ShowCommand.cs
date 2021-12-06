using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Helpers;
using Xs.Tools;

namespace Xs.Commands.Remote;

internal class ShowCommand : Command<DiscoverConfiguration>
{
    public override string Id => "show";
    public override string Description => "Show information about tracked registry.";
    private readonly IConfigurationManager _configurationManager;

    public ShowCommand(
        IConfigurationManager configurationManager
    )
    {
        _configurationManager = configurationManager;
    }

    public override void Handle(
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var dir = discoverCfg.Root;

        var configuration = _configurationManager.Load(dir);

        if (configuration == null)
            Console.WriteLine("Registry is not tracked.");
        else
            Console.Write(Yaml.Serializer.Serialize(configuration));
    }
}