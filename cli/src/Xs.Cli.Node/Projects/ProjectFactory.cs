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

namespace Xs.Cli.Node.Projects
{
    internal class ProjectFactory : SpecialProjectFactoryBase<ISpecialProject>, ISpecialProjectFactory
    {
        public ProjectType Type { get; } = Constants.ProjectType;
        public static readonly string[] TrackedFileExtensions = new [] { ".html", ".ts", ".tsx", ".js", ".scss", ".css", ".json" };
        public static readonly string[] IgnoredFolders = new [] { "build", "dist", ModulesDirectory };
        public const string ModulesDirectory = "node_modules";
        public const string ProjectFileName = "package.json";
        public const string LockFileName = "yarn.lock";
        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;
        private readonly ProjectMapper mapper;
        private readonly LoggerConfiguration loggerConfiguration;
        private readonly IShell shell;
        private readonly IServiceProvider provider;

        public ProjectFactory(
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            LoggerConfiguration loggerConfiguration,
            IShell shell,
            IServiceProvider provider
        )
        {
            this.auditRules = auditRules;
            this.mapper = mapper;
            this.loggerConfiguration = loggerConfiguration;
            this.shell = shell;
            this.provider = provider;
        }

        public bool IsProjectDirectory(string directory)
        {
            // considered project directory, if path doesn't contain modulesDirectory
            // and it's only one in all subdirectories, except those in modulesDirectory

            return Directory.Exists(directory) &&
                !directory.Contains(ModulesDirectory) &&
                Directory.GetFiles(directory, ProjectFileName).Length == 1 &&
                !FileManager.FindDirectory(directory, isMatch, IgnoredFolders);

            static bool isMatch(string dir) => Directory.GetFiles(dir, ProjectFileName).Length > 0;
        }

        public bool IsProjectFile(string file)
        {
            if (!file.EndsWith(ProjectFileName))
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
            var file = new FileInfo(Path.Combine(directory, ProjectFileName));
            var(name, version, description, projectDeps, packageDeps, scripts, isPackable) = mapper.Load(file.FullName, configuration);

            var projectDependencies = projectDeps
                .Select(e => GetProjectDependencyMock(file, e))
                .ToHashSet();

            var packageDependencies = packageDeps.ToHashSet();

            var isTestProject = scripts.ContainsKey("test");

            if (isPackable && isTestProject)
                return new LibraryTestProject(getContext<LibraryTestProject>());

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
                    scripts,
                    shell,
                    loggerConfiguration,
                    provider.GetRequiredService<ILogger<TProject>>(),
                    auditRules,
                    mapper
                );
        }
    }
}