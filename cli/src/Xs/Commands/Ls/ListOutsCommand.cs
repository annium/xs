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

internal class ListOutsCommand : AsyncCommand<ListOutsCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "outs";
    public static string Description => "List projects and their project dependents.";
    private readonly DiscoverProjectsTask _discoverTask;

    public ListOutsCommand(DiscoverProjectsTask discoverTask)
    {
        _discoverTask = discoverTask;
    }

    public override async Task HandleAsync(
        ListOutsCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterType(cfg.Type).FilterMask(cfg.Mask).ToArray();

        // log projects' dependants, if matching projects found
        if (projects.Length > 0)
        {
            var last = projects.Last();

            // if plain dependants list requested - them in single list
            if (cfg.Depth == 0)
            {
                LogPlainDependants(projects, allProjects, cfg);
                return;
            }

            foreach (var project in projects)
                LogProjectWithDependants(
                    new Dependency<IProject>(DependencyType.Normal, project),
                    allProjects,
                    string.Empty,
                    cfg.Depth,
                    project == last
                );

            return;
        }

        // if no projects - search for packages
        var packages = allProjects
            .SelectMany(p => p.Packages)
            .Select(d => d.Value)
            .Distinct()
            .FilterType(cfg.Type)
            .FilterMask(cfg.Mask)
            .ToArray();

        if (packages.Length > 0)
        {
            if (cfg.Depth == 0)
            {
                var dependants = allProjects
                    .Where(p => p.Packages.Any(d => packages.Contains(d.Value)))
                    .Distinct()
                    .OrderBy(p => p.Name)
                    .ToArray();
                foreach (var dependant in dependants)
                    Console.WriteLine(dependant.Describe(cfg.Path, cfg.Attributes));
                return;
            }

            foreach (var package in packages)
            {
                Console.WriteLine(package);
                var dependants = allProjects.Where(p => p.Packages.Any(d => d.Value == package)).ToArray();

                var last = dependants.Last();

                foreach (var dependant in dependants)
                    LogProjectWithDependants(
                        new Dependency<IProject>(DependencyType.Normal, dependant),
                        allProjects,
                        string.Empty,
                        cfg.Depth,
                        dependant == last
                    );
            }

            return;
        }

        Console.WriteLine("No projects/packages, matching given type/mask, found");
    }

    private void LogPlainDependants(
        IEnumerable<IProject> projects,
        IEnumerable<IProject> allProjects,
        ListOutsCommandConfiguration cfg
    )
    {
        var dependants = allProjects
            .Where(e => e.Projects.Select(p => p.Value).Intersect(projects).Any())
            .OrderBy(e => e.Name)
            .ToArray();
        foreach (var dependant in dependants)
            Console.WriteLine(dependant.Describe(cfg.Path, cfg.Attributes));
    }

    private void LogProjectWithDependants(
        Dependency<IProject> projectDependency,
        IReadOnlyCollection<IProject> projects,
        string prefix,
        int nest,
        bool isLast
    )
    {
        var (dependencyType, project) = projectDependency;
        var dependants = projects
            .Select(e =>
            {
                var dep = e.Projects.FirstOrDefault(p => p.Value == project);

                return dep is null ? null : new Dependency<IProject>(dep.Type, e);
            })
            .OfType<Dependency<IProject>>()
            .OrderBy(e => e.Value.Name)
            .ToArray();
        var node = isLast ? "└─" : "├─";
        if (dependants.Length == 0 || nest == 0)
        {
            Console.WriteLine($"{prefix}{node}─ {project} {project.Version} ({dependencyType})");
            return;
        }

        Console.WriteLine($"{prefix}{node}┬ {project} {project.Version} ({dependencyType})");
        prefix += isLast ? "  " : "│ ";
        var last = dependants.Last();
        foreach (var dependant in dependants)
            LogProjectWithDependants(dependant, projects, prefix, nest - 1, dependant == last);
    }
}

internal class ListOutsCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects/packages mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project/package type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option("n")]
    [Help("Show with given depth of recursion.")]
    public int Depth { get; set; } = int.MaxValue;

    [Option]
    [Help("Show path instead of name.")]
    public bool Path { get; set; } = false;

    [Option("a")]
    [Help("Show project attributes.")]
    public bool Attributes { get; set; } = false;
}
