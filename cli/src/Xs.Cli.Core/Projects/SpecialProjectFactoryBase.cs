using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public class SpecialProjectFactoryBase<TProject> where TProject : class, IProject
    {
        protected Dependency<IProject> GetProjectDependencyMock(
            string project,
            FileInfo location,
            Dependency<string> reference
        )
        {
            var file = Path.GetFullPath(Path.Combine(location.DirectoryName, reference.Value));
            var directory = Directory.GetParent(file).FullName;

            var dependency = new ProjectMock<TProject>(
                location.Name,
                new Models.Version(0, 0, 0, string.Empty),
                string.Empty,
                directory,
                file
            );

            return new Dependency<IProject>(reference.Type, dependency);
        }

        // TODO: use in linker

        // protected Dependency<IProject> ResolveProjectDependency(
        //     string project,
        //     FileInfo location,
        //     Dependency<string> reference,
        //     IEnumerable<IProject> projects
        // )
        // {
        //     var directory = Directory.GetParent(Path.GetFullPath(Path.Combine(location.DirectoryName, reference.Value))).FullName;

        //     var dependency = projects.OfType<TProject>()
        //         .FirstOrDefault(e => e.Directory == directory);

        //     if (dependency == null)
        //         throw new InvalidOperationException($"Project {project} has unresolved project dependency {reference}.");

        //     return new Dependency<IProject>(reference.Type, dependency);
        // }

        // protected static Dependency<Package> ResolvePackageDependency(
        //     string project,
        //     Dependency<Package> dep
        // )
        // {
        //     var raw = dep.Value;
        //     var(_, name, version) = raw;
        //     var nameLow = name.ToLowerInvariant();

        //     var dependency = packages.FirstOrDefault(e => e.Name.ToLowerInvariant() == nameLow) ??
        //         raw;

        //     if (!configuration.IgnoreConsistency && name != dependency.Name)
        //         throw new InvalidOperationException($"Project {project} uses different package naming: {name} != {dependency.Name}.");

        //     if (!configuration.IgnoreConsistency && version != dependency.Version)
        //         throw new InvalidOperationException($"Project {project} uses different package {name} version: {version} != {dependency.Version}.");

        //     return new Dependency<Package>(dep.Type, raw);
        // }
    }
}