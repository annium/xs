using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands;

internal class FormatCommand
    : AsyncCommand<FormatCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "format";
    public static string Description => "Format projects.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;

    public FormatCommand(DiscoverProjectsTask discoverTask, ILogger logger)
    {
        _discoverTask = discoverTask;
        Logger = logger;
    }

    public override async Task HandleAsync(
        FormatCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterMask(cfg.Mask).FilterType(cfg.Type).ToArray();

        this.Debug($"Format {projects} project(s)");
        foreach (var project in projects)
        {
            this.Debug($"Format {project}");
            project.Save();
        }
    }
}

internal class FormatCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;
}
