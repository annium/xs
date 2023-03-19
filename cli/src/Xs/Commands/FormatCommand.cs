using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Threading.Tasks;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands;

internal class FormatCommand : Command<FormatCommandConfiguration, DiscoverConfiguration>, ILogSubject<FormatCommand>
{
    public override string Id => "format";
    public override string Description => "Format projects.";
    public ILogger<FormatCommand> Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;

    public FormatCommand(
        DiscoverProjectsTask discoverTask,
        ILogger<FormatCommand> logger
    )
    {
        _discoverTask = discoverTask;
        Logger = logger;
    }

    public override void Handle(
        FormatCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var projects = _discoverTask.RunAsync(discoverCfg).Await()
            .FilterMask(cfg.Mask)
            .FilterType(cfg.Type)
            .ToArray();

        this.Log().Debug($"Format {projects} project(s)");
        foreach (var project in projects)
        {
            this.Log().Debug($"Format {project}");
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