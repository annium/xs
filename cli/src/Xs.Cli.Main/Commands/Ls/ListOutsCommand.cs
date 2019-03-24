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
    internal class ListOutsCommand : Command<ListOutsCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "outs";

        public override string Description { get; } = "List projects and their project dependents.";

        private readonly DiscoverProjectsTask discoverTask;

        public ListOutsCommand(
            DiscoverProjectsTask discoverTask
        )
        {
            this.discoverTask = discoverTask;
        }

        public override void Handle(
            ListOutsCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var allProjects = discoverTask.Run(discoverCfg).ToArray();
            var projects = allProjects
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();
            var last = projects.Last();

            // if plain dependants list requested - them in single list
            if (cfg.Plain)
            {
                LogPlainDependants(projects, allProjects);
                return;
            }

            foreach (var project in projects)
                LogProjectWithDependants(project, allProjects, string.Empty, project == last);
        }

        private void LogPlainDependants(IEnumerable<IProject> projects, IEnumerable<IProject> allProjects)
        {
            var dependants = allProjects
                .Where(e => e.Projects.Select(p => p.Value).Intersect(projects).Count() > 0)
                .OrderBy(e => e.Name)
                .ToArray();
            foreach (var dependant in dependants)
                Console.WriteLine(dependant);
        }

        private void LogProjectWithDependants(
            IProject project,
            IEnumerable<IProject> projects,
            string prefix,
            bool isLast
        )
        {
            var dependants = projects
                .Where(e => e.Projects.Any(p => p.Value == project))
                .OrderBy(e => e.Name)
                .ToArray();
            var node = isLast ? "└─" : "├─";
            if (dependants.Length == 0)
            {
                Console.WriteLine($"{prefix}{node}─ {project}");
                return;
            }

            Console.WriteLine($"{prefix}{node}┬ {project}");
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

        private void LogPackage(
            Dependency<Package> package,
            string prefix,
            bool isLast
        )
        {
            var node = isLast ? "└─" : "├─";
            Console.WriteLine($"{prefix}{node}─ {package} ({package.Type})");
        }
    }

    internal class ListOutsCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option]
        [Help("Show plain dependants list (no recursion).")]
        public bool Plain { get; set; }
    }
}