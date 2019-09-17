using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tasks.Dependencies;

namespace Xs.Commands
{
    internal class AddCommand : Command<AddCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "add";
        public override string Description { get; } = "Add dependency to projects.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly AddPackageDependencyTask addPackageDependencyTask;
        private readonly AddProjectDependencyTask addProjectDependencyTask;
        private readonly ILogger<AddCommand> logger;

        public AddCommand(
            DiscoverProjectsTask discoverTask,
            AddPackageDependencyTask addPackageDependencyTask,
            AddProjectDependencyTask addProjectDependencyTask,
            ILogger<AddCommand> logger
        )
        {
            this.addPackageDependencyTask = addPackageDependencyTask;
            this.addProjectDependencyTask = addProjectDependencyTask;
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override void Handle(
            AddCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var name = cfg.Name;
            var version = cfg.Version;
            var type = cfg.Type;

            var allProjects = discoverTask.Run(discoverCfg);
            var allPackages = allProjects.SelectMany(e => e.Packages).Select(d => d.Value).Distinct()
                .GroupBy(p => p.Type)
                .ToDictionary(g => g.Key, g => g.ToArray());

            var targets = allProjects.FilterMask(cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                logger.Info($"No projects found to add dependency to.");
                return;
            }

            logger.Debug($"Try add dependency {name} to {targets.Length} projects.");

            var projects = allProjects.FilterMask(name).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                    addProjectDependencyTask.Run(targets.FilterType(project.Type).ToArray(), new Dependency<IProject>(type, project));

                return;
            }

            logger.Debug($"Assume dependency {name} as package.");
            var packages = ProjectType.List().Where(t => targets.Count(p => p.Type == t) > 0).ToDictionary(
                t => t,
                t => allPackages[t].FilterMask(name).FirstOrDefault()
            );

            // if at least one package not found
            if (packages.Values.Any(e => e == null))
            {
                // if no version - can't define dependency
                if (version == null)
                    throw new InvalidOperationException(
                        $"Package {name} is not used in {packages.First(p => p.Value == null).Key} target projects. Specify version to use."
                    );
                // else - new dependencies is added
                else
                    packages = packages.ToDictionary(e => e.Key, e => new Package(e.Key, name, version));
            }

            // if package already exists: if version exists - check it's same, otherwise - nothing to do.
            else if (version != null && packages.Values.Any(e => e.Version != version))
                throw new ArgumentException($"Package {name} is already used with different version. Specify already used version, or narrow projects mask.");

            foreach (var package in packages.Values)
                addPackageDependencyTask.Run(targets.FilterType(package.Type).ToArray(), new Dependency<Package>(type, package));
        }
    }

    internal class AddCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; }

        [Position(2)]
        [Help("Dependency name.")]
        public string Name { get; set; }

        [Position(3, isRequired : false)]
        [Help("Dependency version (for package dependencies).")]
        public Cli.Core.Models.Version Version { get; set; }

        [Position(4, isRequired : false)]
        [Help("Dependency type.")]
        public DependencyType Type { get; set; } = DependencyType.Normal;
    }
}