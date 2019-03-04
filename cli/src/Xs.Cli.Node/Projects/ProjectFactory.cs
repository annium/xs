using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xs.Cli.Core.Audit;
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

        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;

        private readonly ProjectMapper mapper;

        private readonly LoggerConfiguration loggerConfiguration;

        private readonly ILogger logger;

        private readonly IShell shell;

        public ProjectFactory(
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            LoggerConfiguration loggerConfiguration,
            ILogger logger,
            IShell shell
        )
        {
            this.auditRules = auditRules;
            this.mapper = mapper;
            this.loggerConfiguration = loggerConfiguration;
            this.logger = logger;
            this.shell = shell;
        }

        public bool IsProjectDirectory(string directory)
        {
            // considered project directory, if path doesn't contain modulesDirectory
            // and it's only one in all subdirectories, except those in modulesDirectory

            return Directory.Exists(directory) &&
                !directory.Contains(ModulesDirectory) &&
                Directory.GetFiles(directory, ProjectFileName).Length == 1 &&
                !FileManager.FindDirectory(directory, isMatch, IgnoredFolders);

            bool isMatch(string dir) => Directory.GetFiles(dir, ProjectFileName).Length > 0;
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
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies
        )
        {
            var file = new FileInfo(Path.Combine(directory, ProjectFileName));
            var(name, version, description, projectDeps, packageDeps, scripts) = mapper.Load(file.FullName);

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
                auditRules,
                mapper
            );

            if (scripts.ContainsKey("test"))
                return new TestProject(context);

            return new LibraryProject(context);
        }
    }
}