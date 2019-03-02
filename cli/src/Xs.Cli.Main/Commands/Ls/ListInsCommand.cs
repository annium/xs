using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands.Ls
{
    internal class ListInsCommand : Command<ListInsCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "ins";

        public override string Description { get; } = "List projects and their dependencies.";

        private readonly DiscoverProjectsTask discoverTask;

        public ListInsCommand(
            DiscoverProjectsTask discoverTask
        )
        {
            this.discoverTask = discoverTask;
        }

        public override void Handle(
            ListInsCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(cwdCfg.Cwd)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();

            // show deps if explicitly specified, or opposite flag not set
            var showProjects = cfg.Projects || !cfg.Packages;
            var showPackages = cfg.Packages || !cfg.Projects;

            // if plain dependencies list requested - join deps and log them in single list
            if (cfg.Plain)
            {
                LogPlainDependencies(projects, showProjects, showPackages);
                return;
            }

            // otherwise - log nice dependencies tree
            var last = projects.Last();
            foreach (var project in projects)
                LogProjectWithDependencies(project, showProjects, showPackages, string.Empty, project == last);
        }

        private void LogPlainDependencies(
            IEnumerable<IProject> projects,
            bool showProjects,
            bool showPackages
        )
        {
            if (showProjects)
            {
                var projectDeps = projects.SelectMany(p => p.ProjectDependencies).Distinct().OrderBy(e => e.Name).ToArray();
                foreach (var dependency in projectDeps)
                    Console.WriteLine(dependency);
            }

            if (showPackages)
            {
                var packageDeps = projects.SelectMany(p => p.PackageDependencies).Distinct().OrderBy(e => e.Name).ToArray();
                foreach (var dependency in packageDeps)
                    Console.WriteLine(dependency);
            }
        }

        private void LogProjectWithDependencies(
            IProject project,
            bool showProjects,
            bool showPackages,
            string prefix,
            bool isLast
        )
        {
            var packageDeps = project.PackageDependencies.OrderBy(e => e.Name).ToArray();
            var projectDeps = project.ProjectDependencies.OrderBy(e => e.Name).ToArray();
            var node = isLast ? "└─" : "├─";

            var depsCount = (showProjects ? projectDeps.Length : 0) + (showPackages ? packageDeps.Length : 0);
            if (depsCount == 0)
            {
                Console.WriteLine($"{prefix}{node}─ {project.Name}");
                return;
            }

            Console.WriteLine($"{prefix}{node}┬ {project.Name}");
            prefix += isLast ? "  " : "│ ";

            if (showPackages && packageDeps.Length > 0)
            {
                var last = projectDeps.Length > 0 ? null : packageDeps.Last();
                foreach (var dependency in packageDeps)
                    LogDependency(dependency, prefix, dependency == last);
            }

            if (showProjects && projectDeps.Length > 0)
            {
                var last = projectDeps.Last();
                foreach (var dependency in projectDeps)
                    LogProjectWithDependencies(dependency, showProjects, showPackages, prefix, dependency == last);
            }
        }

        private void LogDependency(
            Dependency dependency,
            string prefix,
            bool isLast
        )
        {
            var node = isLast ? "└─" : "├─";
            Console.WriteLine($"{prefix}{node}─ {dependency}");
        }
    }

    internal class ListInsCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option]
        [Help("Show only project dependencies (without packages).")]
        public bool Projects { get; set; }

        [Option]
        [Help("Show only package dependencies (without projects).")]
        public bool Packages { get; set; }

        [Option]
        [Help("Show plain dependencies list (no recursion).")]
        public bool Plain { get; set; }
    }
}