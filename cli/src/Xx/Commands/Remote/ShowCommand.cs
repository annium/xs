using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Helpers;
using Xx.Cli.Core.Tools;

namespace Xx.Commands.Remote;

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
