using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Core.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class ProjectFactory : ISpecialProjectFactory
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public static readonly string[] TrackedFileExtensions = new [] { ".cs" };

        public static readonly string[] IgnoredFolders = new [] { "bin", "obj" };

        public const string ProjectFileExtension = ".csproj";

        private const string TargetFramework = "netcoreapp2.2";

        private const string projectFileMask = "*.csproj";

        private readonly ProjectMapper mapper;

        private readonly ILogger logger;

        private readonly IShell shell;

        public ProjectFactory(
            ProjectMapper mapper,
            ILogger logger,
            IShell shell
        )
        {
            this.mapper = mapper;
            this.logger = logger;
            this.shell = shell;
        }

        public bool IsProjectDirectory(string directory)
        {
            // considered project directory, if in current directory there's single project file
            // and it's only one in all subdirectories
            return Directory.GetFiles(directory, projectFileMask, SearchOption.TopDirectoryOnly).Length == 1 &&
                Directory.GetFiles(directory, projectFileMask, SearchOption.AllDirectories).Length == 1;
        }

        public bool IsProjectFile(string file)
        {
            return file.EndsWith(ProjectFileExtension) && IsProjectDirectory(Directory.GetParent(file).FullName);
        }

        public bool IsTrackablePath(string path)
        {
            // is tracked, if it's project file or one of tracked file by extension and not in ignore folder
            return (path.EndsWith(ProjectFileExtension) || TrackedFileExtensions.Any(path.EndsWith)) &&
                !IgnoredFolders.Any(path.Contains);
        }

        public IProject CreateProject(
            string directory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies
        )
        {
            var file = new FileInfo(Directory.GetFiles(directory, projectFileMask, SearchOption.TopDirectoryOnly).First());
            var (name, targetFramework, outputType, projectDependencies, packageDependencies) = mapper.Load(file.FullName);

            // check TargetFramework consistency
            if (projects.OfType<ISpecialProject>().Any(e => e.TargetFramework != targetFramework))
                throw new InvalidOperationException($"Project {name} uses different target framework");

            var projectDeps = projectDependencies
                .Select(e => ResolveProjectDependency(name, file, e, projects))
                .ToHashSet();

            var packageDeps = packageDependencies
                .Select(e => ResolvePackageDependency(name, e, dependencies))
                .ToHashSet();

            if (packageDeps.Any(e => e.Name == "Microsoft.NET.Test.Sdk"))
                return new TestProject(name, file, targetFramework, outputType, projectDeps, packageDeps, mapper, shell, logger);

            return new LibraryProject(name, file, targetFramework, outputType, projectDeps, packageDeps, mapper, shell, logger);
        }

        private IProject ResolveProjectDependency(
            string project,
            FileInfo location,
            string reference,
            IEnumerable<IProject> projects
        )
        {
            var directory = Directory.GetParent(Path.Combine(location.DirectoryName, reference)).FullName;

            var dependency = projects.OfType<ISpecialProject>()
                .FirstOrDefault(e => e.File.DirectoryName == directory);

            if (dependency == null)
                throw new InvalidOperationException($"Project {project} has unresolved project dependency {reference}");

            return dependency;
        }

        private static Dependency ResolvePackageDependency(
            string project,
            Dependency raw,
            IEnumerable<Dependency> dependencies
        )
        {
            var dependency = dependencies.FirstOrDefault(e => e.Name.ToLowerInvariant() == raw.Name.ToLowerInvariant()) ??
                raw;

            if (raw.Name != dependency.Name)
                throw new InvalidOperationException($"Project {project} uses different dependency naming: {raw.Name} -> {dependency.Name}");

            if (!raw.Version.Equals(dependency.Version))
                throw new InvalidOperationException($"Project {project} uses different dependency {raw.Name} version: {raw.Version} -> {dependency.Version}");

            return dependency;
        }
    }
}