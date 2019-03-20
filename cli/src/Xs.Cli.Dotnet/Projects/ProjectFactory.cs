using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Dotnet.Projects
{
    internal class ProjectFactory : SpecialProjectFactoryBase<ISpecialProject>, ISpecialProjectFactory
    {
        public const string ProjectFileExtension = ".csproj";

        public const string TestSDK = "Microsoft.NET.Test.Sdk";

        public const string TestCoveragePackage = "coverlet.msbuild";

        public static readonly string[] TrackedFileExtensions = new [] { ".cs" };

        public static readonly string[] IgnoredFolders = new [] { "bin", "obj" };

        private const string projectFileMask = "*.csproj";

        private static readonly string[] TestDependencies = new [] { TestSDK, TestCoveragePackage };

        public ProjectType Type { get; } = Constants.ProjectType;

        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;

        private readonly ProjectMapper mapper;

        private readonly IShell shell;

        private readonly LoggerConfiguration loggerConfiguration;

        private readonly ILogger logger;

        public ProjectFactory(
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            LoggerConfiguration loggerConfiguration,
            ILogger logger
        )
        {
            this.auditRules = auditRules;
            this.mapper = mapper;
            this.shell = shell;
            this.loggerConfiguration = loggerConfiguration;
            this.logger = logger;
        }

        public bool IsProjectDirectory(string directory)
        {
            // considered project directory, if in current directory there's single project file
            // and it's only one in all subdirectories
            return Directory.Exists(directory) &&
                Directory.GetFiles(directory, projectFileMask).Length == 1 &&
                !FileManager.FindDirectory(directory, isMatch, IgnoredFolders);

            bool isMatch(string dir) => Directory.GetFiles(dir, projectFileMask).Length > 0;
        }

        public bool IsProjectFile(string file)
        {
            if (!file.EndsWith(ProjectFileExtension))
                return false;

            var directory = Directory.GetParent(file).FullName;
            if (FileManager.IsUnrootedDirectoryIgnored(directory, IgnoredFolders))
                return false;

            return IsProjectDirectory(directory);
        }

        public IProject CreateProject(
            string directory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies,
            DiscoverConfiguration configuration
        )
        {
            var file = new FileInfo(Directory.GetFiles(directory, projectFileMask, SearchOption.TopDirectoryOnly).First());
            var(name, version, description, targetFramework, outputType, projectDeps, packageDeps, isPackable) = mapper.Load(file.FullName, configuration);

            // check TargetFramework consistency
            if (projects.OfType<ISpecialProject>().Any(e => e.TargetFramework != targetFramework))
                throw new InvalidOperationException($"Project {name} uses different target framework.");

            var projectDependencies = projectDeps
                .Select(e => ResolveProjectDependency(name, file, e, projects))
                .ToHashSet();

            var packageDependencies = packageDeps
                .Select(e => ResolvePackageDependency(name, e, dependencies))
                .ToHashSet();

            var context = new SpecialProjectContext(
                Constants.ProjectType,
                name,
                version,
                description,
                file,
                projectDependencies,
                packageDependencies,
                shell,
                loggerConfiguration,
                logger,
                targetFramework,
                outputType,
                auditRules,
                mapper
            );

            var isTestProject = configuration.SkipChecks ?
                packageDependencies.Any(d => d.Name == TestSDK) :
                TestDependencies.All(d => packageDependencies.Any(e => e.Name == d));

            if (isTestProject)
                return new TestProject(context);

            if (isPackable)
                return new LibraryProject(context);

            return new BaseProject(context);
        }
    }
}