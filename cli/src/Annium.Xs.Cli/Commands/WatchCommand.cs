using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Logging;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Tools;

namespace Annium.Xs.Cli.Commands;

internal class WatchCommand
    : AsyncCommand<WatchCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "watch";
    public static string Description => "Watch projects' changes and install/build/test on fly.";
    public ILogger Logger { get; }
    private readonly IProjectFactory _projectFactory;
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ProjectsRunner _runner;
    private readonly Watcher _watcher;
    private readonly IShell _shell;
    private readonly LoggerConfiguration _loggerConfiguration;
    private string _mask = string.Empty;
    private ProjectType _type = ProjectType.None;
    private string _command = string.Empty;
    private bool _force;
    private bool _runTests;
    private string _testFilter = string.Empty;
    private DiscoverConfiguration _discoverCfg = new();
    private CancellationToken _token;
    private IProject[] _projects = [];

    public WatchCommand(
        IProjectFactory projectFactory,
        DiscoverProjectsTask discoverTask,
        ProjectsRunner runner,
        Watcher watcher,
        IShell shell,
        ILogger logger,
        LoggerConfiguration loggerConfiguration
    )
    {
        _projectFactory = projectFactory;
        _discoverTask = discoverTask;
        _runner = runner;
        _watcher = watcher;
        _shell = shell;
        Logger = logger;
        _loggerConfiguration = loggerConfiguration;
    }

    public override async Task HandleAsync(
        WatchCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        _mask = cfg.Mask;
        _type = cfg.Type;
        _command = cfg.Command;
        _force = cfg.Force;
        _runTests = cfg.Test || !string.IsNullOrWhiteSpace(cfg.TestFilter);
        _testFilter = cfg.TestFilter;
        _discoverCfg = discoverCfg;
        _token = ct;

        await DiscoverAsync();

        if (string.IsNullOrWhiteSpace(_command))
            await _watcher.WatchAsync(discoverCfg.Root, FilterChange, HandleChangeAsync, HandleDeleteAsync, ct);
        else
            await _watcher.WatchAsync(discoverCfg.Root, FilterChange, CallCommandAsync, CallCommandAsync, ct);
    }

    private bool FilterChange(string path) =>
        _projectFactory.IsProjectFile(path) || _projects.Any(e => e.IsRelated(path));

    private async Task HandleChangeAsync(string path)
    {
        var isProjectFile = _projectFactory.IsProjectFile(path);
        IProject? project;

        if (isProjectFile)
        {
            this.Info<string>("Changed project file: {path}", path);
            await DiscoverAsync();

            project = GetProjectByPath(path);
            if (project is not null)
                await InstallAsync(project, includeSelf: true);
        }
        else
            project = GetProjectByRelatedPath(path);

        if (project is null)
            return;

        this.Info<IProject, string>("Changed {project} related file: {path}", project, path);

        await BuildAsync(project, includeSelf: true);
        if (_runTests)
            await TestAsync(project, includeSelf: true);

        this.Info("Done.");
    }

    private async Task HandleDeleteAsync(string path)
    {
        var project = GetProjectByPath(path);
        var isProjectFile = project is not null;

        if (isProjectFile)
        {
            this.Info<string>("Deleted project file: {path}", path);
            await DiscoverAsync();

            await InstallAsync(project!, includeSelf: false);
        }
        else
            project = GetProjectByRelatedPath(path);

        if (project is null)
            return;

        this.Info<IProject, string>("Deleted {project} related file: {path}", project, path);

        await BuildAsync(project, includeSelf: !isProjectFile);
        if (_runTests)
            await TestAsync(project, includeSelf: !isProjectFile);

        this.Info("Done.");
    }

    private Task InstallAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<IInstallableProject>(project, (p, t) => p.InstallAsync(_force, t), includeSelf);

    private Task BuildAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<IBuildableProject>(project, (p, t) => p.BuildAsync(Env.Development, _force, t), includeSelf);

    private Task TestAsync(IProject project, bool includeSelf) =>
        ExecuteAsync<ITestableProject>(project, (p, t) => p.TestAsync(Env.Development, _testFilter, t), includeSelf);

    private async Task ExecuteAsync<TProject>(
        IProject project,
        Func<TProject, CancellationToken, Task> handle,
        bool includeSelf
    )
        where TProject : IProject
    {
        var selected = CollectDependants(project, includeSelf).OfType<TProject>().ToArray();

        if (selected.Length > 0)
            await _runner.RunAsync(selected, handle, new ProjectsRunner.Config(), _token);
    }

    private IEnumerable<IProject> CollectDependants(IProject project, bool includeSelf)
    {
        var list = new List<IProject>();
        if (includeSelf)
            list.Add(project);

        var dependants = _projects.Where(candidate => candidate.Projects.Any(d => d.Value == project)).ToArray();
        foreach (var dependant in dependants)
            list.AddRange(CollectDependants(dependant, true));

        return list.Distinct();
    }

    private Task CallCommandAsync(string path)
    {
        var result = _shell
            .Cmd(_command.Replace("%", path))
            .Print((LogLevel)_loggerConfiguration <= LogLevel.Debug)
            .Start();

        Task.Run(() => Pipe(result.Output)).GetAwaiter();
        Task.Run(() => Pipe(result.Error)).GetAwaiter();

        return result.Result;

        static void Pipe(StreamReader src)
        {
            while (!src.EndOfStream)
                Console.WriteLine(src.ReadLine());
        }
    }

    private async Task DiscoverAsync()
    {
        var allProjects = await _discoverTask.RunAsync(_discoverCfg);
        var targets = allProjects.FilterMask(_mask).FilterType(_type).OrderByDescending(p => p.Name.Length).ToArray();

        var result = new HashSet<IProject>();
        foreach (var project in targets)
            CollectTargets(project, result);

        _projects = result.ToArray();

        this.Debug("Discovered {count} project(s) to watch:", _projects.Length);
        foreach (var project in _projects)
            this.Debug<string>("{name}", project.Name);
    }

    private void CollectTargets(IProject project, HashSet<IProject> targets)
    {
        // if target not added - it was already handled
        // is used to prevent circular calls
        if (!targets.Add(project))
            return;

        foreach (var dependency in project.Projects.Select(d => d.Value))
            CollectTargets(dependency, targets);
    }

    private IProject? GetProjectByPath(string path) => _projects.FirstOrDefault(e => e.File == path);

    private IProject? GetProjectByRelatedPath(string path) => _projects.FirstOrDefault(e => e.IsRelated(path));
}

internal class WatchCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option("f", isRequired: false)]
    [Help("Force install.")]
    public bool Force { get; set; } = false;

    [Option("t", isRequired: false)]
    [Help("Run tests.")]
    public bool Test { get; set; } = false;

    [Option("tf", isRequired: false)]
    [Help("Tests filter.")]
    public string TestFilter { get; set; } = string.Empty;

    [Raw]
    [Help("Command to execute on change.")]
    public string Command { get; set; } = string.Empty;

    [Option("d")]
    [Help("Watch dependencies.")]
    public bool Deep { get; set; }
}
