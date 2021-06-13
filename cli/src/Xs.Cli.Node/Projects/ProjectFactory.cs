using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
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
        public ProjectType Type => Constants.ProjectType;
        public static readonly string[] TrackedFileExtensions = new[] { ".html", ".ts", ".tsx", ".js", ".scss", ".css", ".json" };
        public static readonly string[] IgnoredFolders = new[] { "build", "dist", ModulesDirectory };
        public const string ModulesDirectory = "node_modules";
        public const string ProjectFileName = "package.json";
        public const string LockFileName = "pnpm-lock.yaml";
        private readonly IEnumerable<IAuditRule<ISpecialProject>> _auditRules;
        private readonly ProjectMapper _mapper;
        private readonly LoggerConfiguration _loggerConfiguration;
        private readonly IShell _shell;
        private readonly IServiceProvider _provider;

        public ProjectFactory(
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            LoggerConfiguration loggerConfiguration,
            IShell shell,
            IServiceProvider provider
        )
        {
            _auditRules = auditRules;
            _mapper = mapper;
            _loggerConfiguration = loggerConfiguration;
            _shell = shell;
            _provider = provider;
        }

        public bool IsProjectDirectory(string directory)
        {
            // considered project directory, if path doesn't contain modulesDirectory
            // and it's only one in all subdirectories, except those in modulesDirectory
            if (!Directory.Exists(directory))
                return false;

            if (directory.Contains(ModulesDirectory))
                return false;

            var projectFiles = Directory.GetFiles(directory, ProjectFileName);
            if (projectFiles.Length != 1)
                return false;

            return !FileManager.FindDirectory(directory, IsMatch, IgnoredFolders);

            static bool IsMatch(string dir) => Directory.GetFiles(dir, ProjectFileName).Length > 0;
        }

        public bool IsProjectFile(string file)
        {
            if (!file.EndsWith(ProjectFileName))
                return false;

            var parent = Directory.GetParent(file) ?? throw new DirectoryNotFoundException($"File {file} has no parent directory");
            var directory = parent.FullName;
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
            var (name, version, description, projectDeps, packageDeps, scripts, isPackable) = _mapper.Load(file.FullName, configuration);

            var projectDependencies = projectDeps
                .Select(e => GetProjectDependencyMock(file, e))
                .ToHashSet();

            var packageDependencies = packageDeps.ToHashSet();

            var isTestProject = scripts.ContainsKey("test");

            if (isPackable && isTestProject)
                return new LibraryTestProject(GetContext<LibraryTestProject>());

            if (isPackable)
                return new LibraryProject(GetContext<LibraryProject>());

            if (isTestProject)
                return new TestProject(GetContext<TestProject>());

            return new SealedProject(GetContext<SealedProject>());

            SpecialProjectContext<TProject> GetContext<TProject>() where TProject : SpecialProject<TProject>
                => new(
                    Constants.ProjectType,
                    name,
                    version,
                    description,
                    directory,
                    projectDependencies,
                    packageDependencies,
                    scripts,
                    _shell,
                    _loggerConfiguration,
                    _provider.Resolve<ILogger<TProject>>(),
                    _auditRules,
                    _mapper
                );
        }
    }
}