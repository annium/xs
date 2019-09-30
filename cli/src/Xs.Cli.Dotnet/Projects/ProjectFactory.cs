using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
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
        public const string TestCoveragePackage = "coverlet.msbuild";
        public static readonly string[] TrackedFileExtensions = new [] { ".cs" };
        public static readonly string[] IgnoredFolders = new [] { "bin", "obj" };
        private const string projectFileMask = "*.csproj";
        public ProjectType Type { get; } = Constants.ProjectType;
        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;
        private readonly ProjectMapper mapper;
        private readonly IShell shell;
        private readonly LoggerConfiguration loggerConfiguration;
        private readonly IServiceProvider provider;

        public ProjectFactory(
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            LoggerConfiguration loggerConfiguration,
            IServiceProvider provider
        )
        {
            this.auditRules = auditRules;
            this.mapper = mapper;
            this.shell = shell;
            this.loggerConfiguration = loggerConfiguration;
            this.provider = provider;
        }

        public bool IsProjectDirectory(string directory)
        {
            // considered project directory, if in current directory there's single project file
            // and it's only one in all subdirectories
            return Directory.Exists(directory) &&
                Directory.GetFiles(directory, projectFileMask).Length == 1 &&
                !FileManager.FindDirectory(directory, isMatch, IgnoredFolders);

            static bool isMatch(string dir) => Directory.GetFiles(dir, projectFileMask).Length > 0;
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
            DiscoverConfiguration configuration
        )
        {
            var file = new FileInfo(Directory.GetFiles(directory, projectFileMask, SearchOption.TopDirectoryOnly).First());
            var(name, version, description, targetFramework, outputType, projectDeps, packageDeps, isPackable, isTestProject) = mapper.Load(file.FullName, configuration);

            var projectDependencies = projectDeps
                .Select(e => GetProjectDependencyMock(file, e))
                .ToHashSet();

            var packageDependencies = packageDeps.ToHashSet();

            if (isPackable)
                return new LibraryProject(getContext<LibraryProject>());

            if (isTestProject)
                return new TestProject(getContext<TestProject>());

            return new SealedProject(getContext<SealedProject>());

            SpecialProjectContext<TProject> getContext<TProject>() where TProject : SpecialProject<TProject>
                => new SpecialProjectContext<TProject>(
                    Constants.ProjectType,
                    name,
                    version,
                    description,
                    directory,
                    projectDependencies,
                    packageDependencies,
                    shell,
                    loggerConfiguration,
                    provider.GetRequiredService<ILogger<TProject>>(),
                    targetFramework,
                    outputType,
                    auditRules,
                    mapper
                );
        }
    }
}