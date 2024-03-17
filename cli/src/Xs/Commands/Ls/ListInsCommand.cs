using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands.Ls;

internal class ListInsCommand : AsyncCommand<ListInsCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "ins";
    public static string Description => "List projects and their dependencies.";
    private readonly DiscoverProjectsTask _discoverTask;

    public ListInsCommand(DiscoverProjectsTask discoverTask)
    {
        _discoverTask = discoverTask;
    }

    public override async Task HandleAsync(
        ListInsCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterMask(cfg.Mask).FilterType(cfg.Type).ToArray();

        // show deps if explicitly specified, or opposite flag not set
        var showProjects = cfg.Projects || !cfg.Packages;
        var showPackages = cfg.Packages || !cfg.Projects;

        // if plain/direct dependencies list requested - join deps and log them in single list
        if (cfg.Plain || cfg.Depth == 0)
        {
            LogPlainDependencies(projects, showProjects, showPackages, cfg);
            return;
        }

        // otherwise - log nice dependencies tree for given depth level
        var nest = cfg.Depth;
        var last = projects.Last();
        foreach (var project in projects)
            LogProjectWithDependencies(
                new Dependency<IProject>(DependencyType.Normal, project),
                showProjects,
                showPackages,
                string.Empty,
                nest,
                project == last
            );
    }

    private void LogPlainDependencies(
        IReadOnlyCollection<IProject> projects,
        bool showProjects,
        bool showPackages,
        ListInsCommandConfiguration cfg
    )
    {
        if (showProjects)
        {
            var projectDeps = projects
                .SelectMany(p => p.Projects)
                .Select(d => d.Value)
                .Distinct()
                .OrderBy(e => e.Name)
                .ToArray();
            foreach (var dependency in projectDeps)
                Console.WriteLine(dependency.Describe(cfg.Path, cfg.Attributes));
        }

        if (showPackages)
        {
            var packageDeps = projects
                .SelectMany(p => p.Packages)
                .Select(d => d.Value)
                .Distinct()
                .OrderBy(e => e.Name)
                .ToArray();
            foreach (var dependency in packageDeps)
                Console.WriteLine(dependency);
        }
    }

    private void LogProjectWithDependencies(
        Dependency<IProject> projectDependency,
        bool showProjects,
        bool showPackages,
        string prefix,
        int nest,
        bool isLast
    )
    {
        var (dependencyType, project) = projectDependency;
        var packageDeps = project.Packages.OrderBy(e => e.Type).ThenBy(e => e.Value.Name).ToArray();
        var projectDeps = project.Projects.OrderBy(e => e.Type).ThenBy(e => e.Value.Name).ToArray();
        var node = isLast ? "└─" : "├─";

        var depsCount = (showProjects ? projectDeps.Length : 0) + (showPackages ? packageDeps.Length : 0);
        if (depsCount == 0 || nest == 0)
        {
            Console.WriteLine($"{prefix}{node}─ {project} {project.Version} ({dependencyType})");
            return;
        }

        Console.WriteLine($"{prefix}{node}┬ {project} {project.Version} ({dependencyType})");
        prefix += isLast ? "  " : "│ ";

        if (showPackages && packageDeps.Length > 0)
        {
            var last = projectDeps.Length > 0 ? null : packageDeps.Last();
            foreach (var dependency in packageDeps)
                LogPackage(dependency, prefix, !(last is null) && dependency == last);
        }

        if (showProjects && projectDeps.Length > 0)
        {
            var last = projectDeps.Last();
            foreach (var dependency in projectDeps)
                LogProjectWithDependencies(dependency, true, showPackages, prefix, nest - 1, dependency == last);
        }
    }

    private void LogPackage(Dependency<Package> package, string prefix, bool isLast)
    {
        var node = isLast ? "└─" : "├─";
        Console.WriteLine($"{prefix}{node}─ {package} ({package.Type})");
    }
}

internal class ListInsCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option]
    [Help("Show only project dependencies (without packages).")]
    public bool Projects { get; set; }

    [Option]
    [Help("Show only package dependencies (without projects).")]
    public bool Packages { get; set; }

    [Option("n")]
    [Help("Show with given depth of recursion.")]
    public int Depth { get; set; } = int.MaxValue;

    [Option]
    [Help("Show path instead of name.")]
    public bool Path { get; set; } = false;

    [Option]
    [Help("Show plain list instead of tree.")]
    public bool Plain { get; set; } = false;

    [Option("a")]
    [Help("Show project attributes.")]
    public bool Attributes { get; set; } = false;
}
