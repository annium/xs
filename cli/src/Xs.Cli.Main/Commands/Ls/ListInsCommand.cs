using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands.Ls
{
    internal class ListInsCommand : AsyncCommand<ListInsCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "ins";

        public override string Description { get; } = "List projects and their dependencies.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        public ListInsCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
        }

        public override async Task HandleAsync(
            ListInsCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var projects = await discoverTask.RunAsync(cwdCfg.Cwd);
            projects = filterTask.Run(projects, cfg.Mask);
            var last = projects.Last();

            foreach (var project in projects)
                LogProjectWithDependencies(project, cfg.ProjectsOnly, string.Empty, project == last);
        }

        private void LogProjectWithDependencies(
            IProject project,
            bool onlyProjects,
            string prefix,
            bool isLast
        )
        {
            var packageDeps = project.PackageDependencies.OrderBy(e => e.Name).ToArray();
            var projectDeps = project.ProjectDependencies.OrderBy(e => e.Name).ToArray();
            var node = isLast ? "└─" : "├─";

            var depsCount = onlyProjects ? projectDeps.Length : packageDeps.Length + projectDeps.Length;
            if (depsCount == 0)
            {
                Console.WriteLine($"{prefix}{node}─ {project.Name}");
                return;
            }

            Console.WriteLine($"{prefix}{node}┬ {project.Name}");
            prefix += isLast ? "  " : "│ ";

            if (!onlyProjects && packageDeps.Length > 0)
            {
                var last = projectDeps.Length > 0 ? null : packageDeps.Last();
                foreach (var dependency in packageDeps)
                    LogDependency(dependency, prefix, dependency == last);
            }

            if (projectDeps.Length > 0)
            {
                var last = projectDeps.Last();
                foreach (var dependency in projectDeps)
                    LogProjectWithDependencies(dependency, onlyProjects, prefix, dependency == last);
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

        [Option("p")]
        [Help("Show only project dependencies (without packages).")]
        public bool ProjectsOnly { get; set; }
    }
}