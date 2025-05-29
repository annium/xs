using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Dotnet.Projects;

namespace Annium.Xs.Cli.Dotnet.Commands.Sln;

public class SetCommand : AsyncCommand<SetCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "set";
    public static string Description => "Create sln file from project.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;

    public SetCommand(DiscoverProjectsTask discoverTask, ILogger logger)
    {
        _discoverTask = discoverTask;
        Logger = logger;
    }

    public override async Task HandleAsync(
        SetCommandConfiguration setCfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var projects = (await _discoverTask.RunAsync(discoverCfg))
            .OfType<IPlatformProject>()
            .FilterMask(setCfg.Mask)
            .ToArray();

        foreach (var project in projects)
        {
            project.Solutions.Add(setCfg.Name);
            project.Save();
        }
    }
}

public class SetCommandConfiguration
{
    [Position(1)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2)]
    [Help("Solution name.")]
    public string Name { get; set; } = string.Empty;
}
