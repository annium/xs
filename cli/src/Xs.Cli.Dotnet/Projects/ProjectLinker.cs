using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Dotnet.Projects
{
    internal class ProjectLinker : SpecialProjectLinkerBase, ISpecialProjectLinker
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public void PreLink(
            IEnumerable<IProject> projects,
            DiscoverConfiguration configuration,
            Action<Exception> addError
        )
        {
            if (!configuration.SkipChecks)
            {
                // check TargetFramework consistency
                var typeProjects = projects.OfType<ISpecialProject>().ToArray();
                if (typeProjects.Select(p => p.TargetFramework).Distinct().Count() > 1)
                    addError(
                        new InvalidOperationException(
                            $"{Type} projects use different target framework:{Environment.NewLine}{string.Join(Environment.NewLine, typeProjects.Select(p => p.TargetFramework))}"
                        )
                    );
            }
        }

        public void Link(
            IProject project,
            IEnumerable<IProject> projects,
            IEnumerable<Package> packages,
            DiscoverConfiguration configuration,
            Action<Exception> addError
        )
        {
            // resolve project dependencies
            var projectDependencies = project.Projects.ToArray();
            project.Projects.Clear();

            foreach (var dependency in projectDependencies)
                project.Projects.Add(ResolveProjectDependency(project, dependency, projects, addError));

            // resolve package dependencies
            var packageDependencies = project.Packages.ToArray();
            project.Packages.Clear();

            foreach (var dependency in packageDependencies)
                project.Packages.Add(ResolvePackageDependency(project, dependency, packages, configuration, addError));
        }
    }
}