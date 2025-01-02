using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;
using Xx.Cli.Core.Tasks;
using Xx.Tools;

namespace Xx.Commands;

internal class BuildCommand
    : AsyncCommand<BuildCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "build";
    public static string Description => "Build projects.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ProjectsRunner _runner;

    public BuildCommand(DiscoverProjectsTask discoverTask, ProjectsRunner runner, ILogger logger)
    {
        _discoverTask = discoverTask;
        _runner = runner;
        Logger = logger;
    }

    public override async Task HandleAsync(
        BuildCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterMask(cfg.Mask).FilterType(cfg.Type).OfType<IBuildableProject>().ToArray();
        this.Debug("Build {length} projects.", projects.Length);
        await _runner.RunAsync(
            projects,
            (project, tkn) => project.BuildAsync(cfg.Env, cfg.Force, tkn),
            new ProjectsRunner.Config(cfg.Parallelism, cfg.Deep),
            ct
        );
    }
}

internal class BuildCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option]
    [Help("Environment.")]
    public Env Env { get; set; } = Env.Development;

    [Option("f")]
    [Help("Force build.")]
    public bool Force { get; set; }

    [Option("d")]
    [Help("Build dependencies.")]
    public bool Deep { get; set; }

    [Option("p")]
    [Help("Degree of parallelism. Default - all available tasks are run in parallel")]
    public int Parallelism { get; set; }
}
