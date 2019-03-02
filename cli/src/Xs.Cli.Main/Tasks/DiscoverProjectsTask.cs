using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tasks
{
    internal class DiscoverProjectsTask
    {
        private readonly IProjectFactory projectFactory;

        private readonly ILogger logger;

        public DiscoverProjectsTask(
            IProjectFactory projectFactory,
            ILogger logger
        )
        {
            this.projectFactory = projectFactory;
            this.logger = logger;
        }

        public IEnumerable<IProject> Run(string root)
        {
            logger.LogDebug($"Start discovery of {root}");

            var directories = FileManager.CollectDirectories(
                root,
                projectFactory.IsProjectDirectory,
                SearchOptions.IgnoreChildrenOnMatch
            );

            var projects = new HashSet<IProject>();
            var dependencies = new HashSet<Dependency>();

            var previous = 0;
            List<Exception> exceptions;
            do
            {
                previous = projects.Count;
                exceptions = new List<Exception>();

                foreach (var directory in directories.ToArray())
                {
                    var(project, exception) = TryCreateProject(directory, projects, dependencies);
                    if (project != null)
                    {
                        directories.Remove(directory);
                        projects.Add(project);
                        logger.LogDebug($"Project discovered: {project}");
                        foreach (var dependency in project.PackageDependencies)
                            dependencies.Add(dependency);
                    }
                    if (exception != null)
                        exceptions.Add(exception);
                }
            }
            while (projects.Count > previous);

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            logger.LogDebug($"Discovery finished. Found {projects.Count} projects.");

            return projects.OrderBy(e => e.Name).ToArray();
        }

        private ValueTuple<IProject, Exception> TryCreateProject(
            string directory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies
        )
        {
            try
            {
                return (projectFactory.CreateProject(directory, projects, dependencies), null);
            }
            catch (Exception exception)
            {
                return (null, exception);
            }
        }
    }
}