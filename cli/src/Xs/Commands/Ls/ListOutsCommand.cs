using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands.Ls
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
                .FilterType(cfg.Type)
                .FilterMask(cfg.Mask)
                .ToArray();

            // log projects' dependants, if matching projects found
            if (projects.Length > 0)
            {
                var last = projects.Last();

                // if plain dependants list requested - them in single list
                if (cfg.Plain)
                {
                    LogPlainDependants(projects, allProjects, cfg);
                    return;
                }

                foreach (var project in projects)
                    LogProjectWithDependants(new Dependency<IProject>(DependencyType.Normal, project), allProjects, string.Empty, project == last);

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
                if (cfg.Plain)
                {
                    var dependants = allProjects
                        .Where(p => p.Packages.Any(d => packages.Contains(d.Value)))
                        .Distinct()
                        .OrderBy(p => p.Name)
                        .ToArray();
                    foreach (var dependant in dependants)
                        LogProject(dependant, cfg.Path, cfg.Attributes);
                    return;
                }

                foreach (var package in packages)
                {
                    Console.WriteLine(package);
                    var dependants = allProjects
                        .Where(p => p.Packages.Any(d => d.Value == package))
                        .ToArray();

                    var last = dependants.Last();

                    foreach (var dependant in dependants)
                        LogProjectWithDependants(new Dependency<IProject>(DependencyType.Normal, dependant), allProjects, string.Empty, dependant == last);

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
                .Where(e => e.Projects.Select(p => p.Value).Intersect(projects).Count() > 0)
                .OrderBy(e => e.Name)
                .ToArray();
            foreach (var dependant in dependants)
                LogProject(dependant, cfg.Path, cfg.Attributes);
        }

        private void LogProjectWithDependants(
            Dependency<IProject> projectDependency,
            IEnumerable<IProject> projects,
            string prefix,
            bool isLast
        )
        {
            var(dependencyType, project) = projectDependency;
            var dependants = projects
                .Select(e =>
                {
                    var dep = e.Projects.FirstOrDefault(p => p.Value == project);

                    return dep == null ? null : new Dependency<IProject>(dep.Type, e);
                })
                .OfType<Dependency<IProject>>()
                .OrderBy(e => e.Value.Name)
                .ToArray();
            var node = isLast ? "└─" : "├─";
            if (dependants.Length == 0)
            {
                Console.WriteLine($"{prefix}{node}─ {project} {project.Version} ({dependencyType})");
                return;
            }

            Console.WriteLine($"{prefix}{node}┬ {project} {project.Version} ({dependencyType})");
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

        private void LogProject(IProject project, bool writePath, bool writeAttributes)
        {
            var sb = new StringBuilder();

            if (writePath)
                sb.Append(project.File);
            else if (writeAttributes)
            {
                sb.Append(project.Name);
                if (project is IPublishableProject)
                    sb.Append(" [Publish]");

                if (project is ITestableProject)
                    sb.Append(" [Test]");
            }
            else
                sb.Append(project.Name);

            Console.WriteLine(sb.ToString());
        }
    }

    internal class ListOutsCommandConfiguration
    {
        [Position(1, isRequired : false)]
        [Help("Projects/packages mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project/package type.")]
        public ProjectType Type { get; set; }

        [Option]
        [Help("Show plain dependants list (no recursion).")]
        public bool Plain { get; set; }

        [Option]
        [Help("Show path instead of name.")]
        public bool Path { get; set; } = false;

        [Option("a")]
        [Help("Show project attributes.")]
        public bool Attributes { get; set; } = false;
    }
}