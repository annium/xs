using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
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
            IProject project,
            Dependency<Package> dep,
            IEnumerable<Package> packages,
            DiscoverConfiguration configuration,
            Action<Package> registerPackage,
            Action<Exception> addError
        )
        {
            var(_, name, version) = dep.Value;
            var nameLow = name.ToLowerInvariant();

            var dependency = packages.FirstOrDefault(e => e.Name.ToLowerInvariant() == nameLow);
            if (dependency is null)
            {
                registerPackage(dep.Value);
                return dep;
            }

            if (!configuration.IgnoreConsistency && name != dependency.Name)
                addError(new InvalidOperationException($"Project {project} uses different package naming: {name} != {dependency.Name}."));

            if (!configuration.IgnoreConsistency && version != dependency.Version)
                addError(new InvalidOperationException($"Project {project} uses different package {name} version: {version} != {dependency.Version}."));

            return new Dependency<Package>(dep.Type, dependency);
        }
    }
}