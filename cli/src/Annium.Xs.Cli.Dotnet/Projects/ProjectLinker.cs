using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Dotnet.Models;

namespace Annium.Xs.Cli.Dotnet.Projects;

internal class ProjectLinker : PlatformProjectLinkerBase, IPlatformProjectLinker
{
    public ProjectType Type => Constants.ProjectType;

    public void PreLink(
        IReadOnlyCollection<IProject> projects,
        DiscoverConfiguration configuration,
        Action<Exception> addError
    )
    {
        if (!configuration.SkipChecks)
        {
            // check TargetFramework consistency
            var typeProjects = projects.OfType<IPlatformProject>().ToArray();
            var frameworks = typeProjects.Select(p => p.TargetFramework).Distinct();
            if (!TargetFramework.SupportedGroups.Any(g => frameworks.All(g.Contains)))
            {
                var usages = string.Join(
                    Environment.NewLine,
                    typeProjects.Select(p => $"{p.Name}: {p.TargetFramework}")
                );
                addError(
                    new InvalidOperationException(
                        $"{Type} projects use incompatible target framework:{Environment.NewLine}{usages}"
                    )
                );
            }
        }
    }

    public void Link(
        IProject project,
        IReadOnlyCollection<IProject> projects,
        IReadOnlyCollection<Package> packages,
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
            project.Packages.Add(ResolvePackageDependency(dependency, packages));
    }
}
