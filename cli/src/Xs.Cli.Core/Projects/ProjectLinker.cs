using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    internal class ProjectLinker : IProjectLinker
    {
        private readonly IEnumerable<ISpecialProjectLinker> linkers;

        public ProjectLinker(IEnumerable<ISpecialProjectLinker> linkers)
        {
            this.linkers = linkers;
        }

        public void PreLink(
            IEnumerable<IProject> projects,
            DiscoverConfiguration configuration,
            Action<Exception> addError
        )
        {
            if (!configuration.IgnoreConsistency && projects.Select(p => p.Version.ToString()).Distinct().Count() > 1)
                addError(new InvalidOperationException(
                    $"Projects use multiple versions:{Environment.NewLine}{string.Join(Environment.NewLine, projects.Select(p => $"{p}: {p.Version}"))}."
                ));

            var projectsByTypes = projects.GroupBy(p => p.Type).ToDictionary(g => g.Key, g => g.ToArray());
            foreach (var(type, typeProjects) in projectsByTypes)
            {
                var linker = linkers.FirstOrDefault(l => l.Type == type);
                if (linker is null)
                    addError(new InvalidOperationException($"No linker found for project type {type}"));
                else
                    linker.PreLink(projects.Where(p => p.Type == type).ToArray(), configuration, addError);
            }
        }

        public void Link(
            IProject project,
            IEnumerable<IProject> projects,
            IEnumerable<Package> packages,
            DiscoverConfiguration configuration,
            Action<Package> registerPackage,
            Action<Exception> addError
        )
        {
            var duplicateProject = projects.FirstOrDefault(p => p != project && p.Name == project.Name);
            if (duplicateProject != null)
            {
                addError(new InvalidOperationException($"Project {project} name is not unique."));
                return;
            }

            if (!configuration.SkipChecks)
            {
                var duplicatePackage = packages.FirstOrDefault(p => p.Name == project.Name);
                if (duplicatePackage != null)
                {
                    addError(new InvalidOperationException($"Project {project} name is used by package {duplicatePackage}."));
                    return;
                }
            }

            var linker = linkers.FirstOrDefault(l => l.Type == project.Type);
            linker.Link(
                project,
                projects,
                packages,
                configuration,
                registerPackage,
                addError
            );
        }
    }
}