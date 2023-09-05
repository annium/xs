using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands;

internal class InstallCommand : AsyncCommand<InstallCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "install";
    public static string Description => "Install projects' dependencies.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ProjectsRunner _runner;

    public InstallCommand(
        DiscoverProjectsTask discoverTask,
        ProjectsRunner runner,
        ILogger logger
    )
    {
        _discoverTask = discoverTask;
        _runner = runner;
        Logger = logger;
    }

    public override async Task HandleAsync(
        InstallCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var force = cfg.Force;

        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects
            .FilterMask(cfg.Mask)
            .FilterType(cfg.Type)
            .ToArray();

        if (force)
        {
            this.Debug($"Clear {projects.Length} projects cache.");
            await _runner.RunAsync(
                projects.OfType<ICachingProject>().ToArray(),
                (project, tkn) => project.ClearCacheAsync(tkn),
                new ProjectsRunner.Config(cfg.Parallelism, cfg.Deep),
                ct
            );
        }

        this.Debug($"Install {projects.Length} projects.");
        await _runner.RunAsync(
            projects.OfType<IInstallableProject>().ToArray(),
            (project, tkn) => project.InstallAsync(force, tkn),
            new ProjectsRunner.Config(cfg.Parallelism, cfg.Deep),
            ct
        );
    }
}

internal class InstallCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option("f", isRequired: false)]
    [Help("Force install.")]
    public bool Force { get; set; }

    [Option("d")]
    [Help("Install dependencies.")]
    public bool Deep { get; set; }

    [Option("p")]
    [Help("Degree of parallelism. Default - all available tasks are run in parallel")]
    public int Parallelism { get; set; }
}