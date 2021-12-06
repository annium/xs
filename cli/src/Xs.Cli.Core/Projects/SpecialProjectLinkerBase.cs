using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

public class SpecialProjectLinkerBase
{
    protected Dependency<IProject> ResolveProjectDependency(
        IProject project,
        Dependency<IProject> mock,
        IEnumerable<IProject> projects,
        Action<Exception> addError
    )
    {
        var directory = mock.Value.Directory;
        var dependency = projects.FirstOrDefault(e => e.Directory == directory);

        if (dependency is null)
        {
            addError(new InvalidOperationException($"Project {project} has unresolved project dependency {mock}."));
            return mock;
        }

        return new Dependency<IProject>(mock.Type, dependency);
    }

    protected static Dependency<Package> ResolvePackageDependency(
        Dependency<Package> dep,
        IEnumerable<Package> packages
    )
    {
        var type = dep.Type;
        var(_, name, _) = dep.Value;
        var nameLow = name.ToLowerInvariant();

        var package = packages
            .Where(e => e.Name.ToLowerInvariant() == nameLow)
            .OrderByDescending(e => e.Version)
            .First();

        return new Dependency<Package>(type, package);
    }
}