using System;
using System.Collections.Generic;
using System.Linq;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;

namespace Xx.Cli.Node.Projects;

internal class ProjectLinker : PlatformProjectLinkerBase, IPlatformProjectLinker
{
    public ProjectType Type => Constants.ProjectType;

    public void PreLink(
        IReadOnlyCollection<IProject> projects,
        DiscoverConfiguration configuration,
        Action<Exception> addError
    ) { }

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
