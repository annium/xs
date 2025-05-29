using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Helpers;
using Annium.Xs.Cli.Core.Tools;

namespace Annium.Xs.Cli.Commands.Remote;

internal class ShowCommand : Command<DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "show";
    public static string Description => "Show information about tracked registry.";
    private readonly IConfigurationManager _configurationManager;

    public ShowCommand(IConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public override void Handle(DiscoverConfiguration discoverCfg, CancellationToken ct)
    {
        var dir = discoverCfg.Root;

        var configuration = _configurationManager.Load(dir);

        Console.Write(Yaml.Serializer.Serialize(configuration));
    }
}
