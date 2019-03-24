using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public class SpecialProjectFactoryBase<TProject> where TProject : IProject
    {
        protected Dependency<IProject> ResolveProjectDependency(
            string project,
            FileInfo location,
            Dependency<string> reference,
            IEnumerable<IProject> projects
        )
        {
            var directory = Directory.GetParent(Path.GetFullPath(Path.Combine(location.DirectoryName, reference.Value))).FullName;

            var dependency = projects.OfType<TProject>()
                .FirstOrDefault(e => e.File.DirectoryName == directory);

            if (dependency == null)
                throw new InvalidOperationException($"Project {project} has unresolved project dependency {reference}.");

            return new Dependency<IProject>(reference.Type, dependency);
        }

        protected static Dependency<Package> ResolvePackageDependency(
            string project,
            Dependency<Package> dep,
            IEnumerable<Package> packages,
            DiscoverConfiguration configuration
        )
        {
            var raw = dep.Value;
            var dependency = packages.FirstOrDefault(e => e.Name.ToLowerInvariant() == raw.Name.ToLowerInvariant()) ??
                raw;

            if (!configuration.IgnoreConsistency && raw.Name != dependency.Name)
                throw new InvalidOperationException($"Project {project} uses different dependency naming: {raw.Name} -> {dependency.Name}.");

            if (!configuration.IgnoreConsistency && !raw.Version.Equals(dependency.Version))
                throw new InvalidOperationException($"Project {project} uses different dependency {raw.Name} version: {raw.Version} -> {dependency.Version}.");

            return new Dependency<Package>(dep.Type, dependency);
        }
    }
}