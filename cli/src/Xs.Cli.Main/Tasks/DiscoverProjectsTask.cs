using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Main.Tasks
{
    internal class DiscoverProjectsTask
    {
        private readonly IProjectFactory projectFactory;
        private readonly ILogger<DiscoverProjectsTask> logger;

        public DiscoverProjectsTask(
            IProjectFactory projectFactory,
            ILogger<DiscoverProjectsTask> logger
        )
        {
            this.projectFactory = projectFactory;
            this.logger = logger;
        }

        public IEnumerable<IProject> Run(DiscoverConfiguration configuration)
        {
            var root = configuration.Root;
            logger.Debug($"Start discovery of {root}");

            var results = new Dictionary<string, ISpecialProjectFactory>();
            FileManager.WalkDirectories(
                root,
                directory =>
                {
                    var factory = projectFactory.FindFactory(directory);
                    if (factory != null)
                        results[directory] = factory;

                    return factory != null;
                },
                SearchOptions.IgnoreChildrenOnMatch
            );

            var projects = new HashSet<IProject>();
            var packages = new HashSet<Package>();

            var previous = 0;
            List<Exception> exceptions;
            do
            {
                previous = projects.Count;
                exceptions = new List<Exception>();

                foreach (var(directory, factory) in results.ToArray())
                {
                    var(project, exception) = TryCreateProject(directory, factory, projects, packages, configuration);
                    if (project != null)
                    {
                        results.Remove(directory);
                        projects.Add(project);
                        logger.Debug($"Project discovered: {project}");
                        foreach (var package in project.Packages)
                            packages.Add(package.Value);
                    }
                    if (exception != null)
                        exceptions.Add(exception);
                }
            }
            while (projects.Count > previous);

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            logger.Debug($"Discovery finished. Found {projects.Count} projects.");

            return projects.OrderBy(e => e.Name).ToArray();
        }

        private ValueTuple<IProject, Exception> TryCreateProject(
            string directory,
            ISpecialProjectFactory factory,
            IEnumerable<IProject> projects,
            IEnumerable<Package> packages,
            DiscoverConfiguration configuration
        )
        {
            try
            {
                return (projectFactory.CreateProject(directory, factory, projects, packages, configuration), null);
            }
            catch (Exception exception)
            {
                return (null, exception);
            }
        }
    }
}