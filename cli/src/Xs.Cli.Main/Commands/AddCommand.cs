using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tasks.Dependencies;

namespace Xs.Cli.Main.Commands
{
    internal class AddCommand : AsyncCommand<AddCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "add";

        public override string Description { get; } = "Add dependency to projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly FilterProjectTypeTask filterTypeTask;

        private readonly AddPackageDependencyTask addPackageDependencyTask;

        private readonly AddProjectDependencyTask addProjectDependencyTask;

        private readonly ILogger logger;

        public AddCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            FilterProjectTypeTask filterTypeTask,
            AddPackageDependencyTask addPackageDependencyTask,
            AddProjectDependencyTask addProjectDependencyTask,
            ILogger logger
        )
        {
            this.addPackageDependencyTask = addPackageDependencyTask;
            this.addProjectDependencyTask = addProjectDependencyTask;
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.filterTypeTask = filterTypeTask;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            AddCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Dependency;
            var nameLow = name.ToLowerInvariant();
            var version = cfg.Version;

            var allProjects = await discoverTask.RunAsync(cwdCfg.Cwd);
            var dependencies = allProjects.SelectMany(e => e.PackageDependencies).Distinct().ToArray();

            var targets = filterTask.Run(allProjects, cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                logger.LogInfo($"No projects found to add dependency to.");
                return;
            }

            logger.LogDebug($"Try add dependency {name} to {targets.Length} projects.");

            var projects = allProjects.Where(e => e.Name.ToLowerInvariant() == nameLow).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                    addProjectDependencyTask.Run(filterTypeTask.Run(targets, project.Type), project);

                return;
            }

            logger.LogDebug($"Assume dependency {name} as package.");
            var packages = ProjectType.List().Where(t => targets.Count(p => p.Type == t) > 0).ToDictionary(
                e => e,
                e => dependencies.FirstOrDefault(d => d.Type == e && d.Name.ToLowerInvariant() == nameLow)
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
                    packages = packages.ToDictionary(e => e.Key, e => new Dependency(e.Key, name, version));
            }

            // if package already exists: if version exists - check it's same, otherwise - nothing to do.
            else if (version != null && packages.Values.Any(e => e.Version != version))
                throw new ArgumentException($"Package {name} is already used with different version. Specify already used version, or narrow projects mask.");

            foreach (var package in packages.Values)
                addPackageDependencyTask.Run(filterTypeTask.Run(targets, package.Type), package);
        }
    }

    internal class AddCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; }

        [Position(2)]
        [Help("Dependency name.")]
        public string Dependency { get; set; }

        [Position(3, isRequired : false)]
        [Help("Dependency version (for package dependencies).")]
        public Core.Models.Version Version { get; set; }
    }
}