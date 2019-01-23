using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands.Ls
{
    internal class ListOutsCommand : AsyncCommand<ListOutsCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "outs";

        public override string Description { get; } = "List projects and their project dependents.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        public ListOutsCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
        }

        public override async Task HandleAsync(
            ListOutsCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var allProjects = await discoverTask.RunAsync(cwdCfg.Cwd);
            var projects = filterTask.Run(allProjects, cfg.Mask);
            var last = projects.Last();

            foreach (var project in projects)
                LogProjectWithDependants(project, allProjects, string.Empty, project == last);
        }

        private void LogProjectWithDependants(
            IProject project,
            IEnumerable<IProject> projects,
            string prefix,
            bool isLast
        )
        {
            var dependants = projects
                .Where(e => e.ProjectDependencies.Contains(project))
                .OrderBy(e => e.Name)
                .ToArray();
            var node = isLast ? "└─" : "├─";
            if (dependants.Length == 0)
            {
                Console.WriteLine($"{prefix}{node}─ {project.Name}");
                return;
            }

            Console.WriteLine($"{prefix}{node}┬ {project.Name}");
            prefix += isLast ? "  " : "│ ";
            var last = dependants.Last();
            foreach (var dependant in dependants)
                LogProjectWithDependants(
                    dependant,
                    projects,
                    prefix,
                    dependant == last
                );
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

    internal class ListOutsCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";
    }
}