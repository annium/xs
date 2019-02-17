using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Dotnet.Projects
{
    internal class ProjectFactory : SpecialProjectFactoryBase<ISpecialProject>, ISpecialProjectFactory
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public static readonly string[] TrackedFileExtensions = new [] { ".cs" };

        public static readonly string[] IgnoredFolders = new [] { "bin", "obj" };

        public const string ProjectFileExtension = ".csproj";

        private const string projectFileMask = "*.csproj";

        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;

        private readonly ProjectMapper mapper;

        private readonly ILogger logger;

        private readonly IShell shell;

        public ProjectFactory(
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            ILogger logger,
            IShell shell
        )
        {
            this.auditRules = auditRules;
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
            var(name, version, description, targetFramework, outputType, projectDependencies, packageDependencies) = mapper.Load(file.FullName);

            // check TargetFramework consistency
            if (projects.OfType<ISpecialProject>().Any(e => e.TargetFramework != targetFramework))
                throw new InvalidOperationException($"Project {name} uses different target framework.");

            var projectDeps = projectDependencies
                .Select(e => ResolveProjectDependency(name, file, e, projects))
                .ToHashSet();

            var packageDeps = packageDependencies
                .Select(e => ResolvePackageDependency(name, e, dependencies))
                .ToHashSet();

            if (packageDeps.Any(e => e.Name == "Microsoft.NET.Test.Sdk"))
                return new TestProject(name, version, description, file, projectDeps, packageDeps, targetFramework, outputType, auditRules, mapper, shell, logger);

            return new LibraryProject(name, version, description, file, projectDeps, packageDeps, targetFramework, outputType, auditRules, mapper, shell, logger);
        }
    }
}