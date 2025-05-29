using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Linq;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tasks;

namespace Annium.Xs.Cli.Commands.Ls;

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

        // show hierarchical projects list
        if (!cfg.Plain || cfg.Depth > 0 && cfg.Depth != int.MaxValue)
        {
            ShowProjectsTree(cfg, projects, showPackages);
            return;
        }

        // if plain/direct dependencies list requested - join deps and log them in single list
        if (showProjects)
            // if plain/direct dependencies list requested - join deps and log them in single list
            ShowPlainProjects(cfg, projects);
        else if (showPackages)
            ShowPlainPackages(cfg, projects);
    }

    private void ShowProjectsTree(
        ListInsCommandConfiguration cfg,
        IReadOnlyCollection<IProject> projects,
        bool showPackages
    )
    {
        // select projects, not references by other projects
        projects = projects.Where(x => projects.None(p => p.Projects.Any(d => d.Value == x))).ToArray();

        var last = projects.Last();
        var ctx = new PrefixedContext(cfg.Depth, showPackages);
        foreach (var project in projects)
        {
            var proj = new Dependency<IProject>(DependencyType.Normal, project);
            Traverse(proj, cfg.Depth, 0, project == last, ctx, ShowProjectNode);
        }

        static void ShowProjectNode(Dependency<IProject> proj, int depth, bool isLast, PrefixedContext ctx)
        {
            var (dependencyType, project) = proj;
            var packageDeps = project.Packages.OrderBy(e => e.Type).ThenBy(e => e.Value.Name).ToArray();
            var projectDeps = project.Projects.OrderBy(e => e.Type).ThenBy(e => e.Value.Name).ToArray();

            {
                var node = isLast ? "└─" : "├─";
                var depsCount = projectDeps.Length + (ctx.ShowPackages ? packageDeps.Length : 0);
                if (depsCount == 0 || ctx.Depth == depth)
                {
                    Console.WriteLine($"{ctx.Prefix}{node}─ {project} {project.Version} ({dependencyType})");
                    return;
                }

                Console.WriteLine($"{ctx.Prefix}{node}┬ {project} {project.Version} ({dependencyType})");
            }

            ctx.ExtendPrefix(isLast ? "  " : "│ ");
            if (ctx.ShowPackages && packageDeps.Length > 0)
            {
                var last = projectDeps.Length > 0 ? null : packageDeps.Last();
                foreach (var dependency in packageDeps)
                {
                    var node = last is not null && dependency == last ? "└─" : "├─";
                    Console.WriteLine($"{ctx.Prefix}{node}─ {dependency.Value} ({dependency.Type})");
                }
            }
        }
    }

    private void ShowPlainProjects(ListInsCommandConfiguration cfg, IProject[] projects)
    {
        var ctx = new CollectContext<IProject>();
        foreach (var project in projects)
        {
            var proj = new Dependency<IProject>(DependencyType.Normal, project);
            Traverse(proj, cfg.Depth, 0, false, ctx, CollectProjects);
        }
        foreach (var dependency in ctx.Items.Except(projects).OrderBy(x => x.Name))
            Console.WriteLine(dependency.Describe(cfg.Path, cfg.Attributes));

        static void CollectProjects(Dependency<IProject> project, int depth, bool isLast, CollectContext<IProject> ctx)
        {
            ctx.Items.Add(project.Value);
        }
    }

    private void ShowPlainPackages(ListInsCommandConfiguration cfg, IProject[] projects)
    {
        var ctx = new CollectContext<Package>();
        foreach (var project in projects)
        {
            var proj = new Dependency<IProject>(DependencyType.Normal, project);
            Traverse(proj, cfg.Depth, 0, false, ctx, CollectPackages);
        }
        foreach (var dependency in ctx.Items.OrderBy(x => x.Name))
            Console.WriteLine(dependency);

        static void CollectPackages(Dependency<IProject> project, int depth, bool isLast, CollectContext<Package> ctx)
        {
            foreach (var dependency in project.Value.Packages)
                ctx.Items.Add(dependency.Value);
        }
    }

    private record CollectContext<T> : ICopyable<CollectContext<T>>
    {
        public HashSet<T> Items { get; } = new();

        public CollectContext<T> Copy() => this;
    }

    private record PrefixedContext(int Depth, bool ShowPackages) : ICopyable<PrefixedContext>
    {
        public string Prefix { get; private set; } = string.Empty;

        public void ExtendPrefix(string prefix)
        {
            Prefix = $"{Prefix}{prefix}";
        }

        public PrefixedContext Copy() => this with { };
    }

    private void Traverse<T>(
        Dependency<IProject> projectDependency,
        int maxDepth,
        int depth,
        bool isLast,
        T ctx,
        Action<Dependency<IProject>, int, bool, T> handle
    )
        where T : ICopyable<T>
    {
        ctx = ctx.Copy();
        handle(projectDependency, depth, isLast, ctx);
        if (depth == maxDepth)
            return;

        var projectDeps = projectDependency.Value.Projects.OrderBy(e => e.Type).ThenBy(e => e.Value.Name).ToArray();
        if (projectDeps.Length <= 0)
            return;

        var last = projectDeps.Last();
        foreach (var dependency in projectDeps)
            Traverse(dependency, maxDepth, depth + 1, dependency == last, ctx, handle);
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
