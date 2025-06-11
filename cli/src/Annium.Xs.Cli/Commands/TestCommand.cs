using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Attributes;
using Annium.Extensions.Arguments.Commands;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Tools;

namespace Annium.Xs.Cli.Commands;

internal class TestCommand
    : AsyncCommand<TestCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "test";
    public static string Description => "Test projects.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ProjectsRunner _runner;

    public TestCommand(DiscoverProjectsTask discoverTask, ProjectsRunner runner, ILogger logger)
    {
        _discoverTask = discoverTask;
        _runner = runner;
        Logger = logger;
    }

    public override async Task HandleAsync(
        TestCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterMask(cfg.Mask).FilterType(cfg.Type).OfType<ITestableProject>().ToArray();

        this.Debug("Test {projectsLength} projects.", projects.Length);
        await _runner.RunAsync(
            projects,
            (project, tkn) => project.TestAsync(cfg.Env, cfg.TestFilter, tkn),
            new ProjectsRunner.Config(cfg.Parallelism, cfg.Deep),
            ct
        );
    }
}

internal class TestCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option("tf", isRequired: false)]
    [Help("Tests filter.")]
    public string TestFilter { get; set; } = string.Empty;

    [Option]
    [Help("Environment.")]
    public Env Env { get; set; } = Env.Development;

    [Option("d")]
    [Help("Test dependencies.")]
    public bool Deep { get; set; }

    [Option("p")]
    [Help("Degree of parallelism. Default - all available tasks are run in parallel")]
    public int Parallelism { get; set; }
}
