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
            // considered project directory, if path doesn't contain modulesDirectory
            // and it's only one in all subdirectories, except those in modulesDirectory
            return !directory.Contains(ModulesDirectory) &&
                Directory.GetFiles(directory, ProjectFileName, SearchOption.TopDirectoryOnly).Length == 1 &&
                Directory.GetFiles(directory, ProjectFileName, SearchOption.AllDirectories)
                .Where(path => !path.Contains(ModulesDirectory))
                .Count() == 1;
        }

        public bool IsProjectFile(string file)
        {
            return file.EndsWith(ProjectFileName) && IsProjectDirectory(Directory.GetParent(file).FullName);
        }

        public bool IsTrackablePath(string path)
        {
            // is tracked, if it's project file or one of tracked file by extension and not in ignore folder
            return (path.EndsWith(ProjectFileName) || TrackedFileExtensions.Any(path.EndsWith)) &&
                !IgnoredFolders.Any(path.Contains);
        }

        public IProject CreateProject(
            string directory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies
        )
        {
            var file = new FileInfo(Path.Combine(directory, ProjectFileName));
            var(name, version, projectDependencies, packageDependencies, scripts) = mapper.Load(file.FullName);

            var projectDeps = projectDependencies
                .Select(e => ResolveProjectDependency(name, file, e, projects))
                .ToHashSet();

            var packageDeps = packageDependencies
                .Select(e => ResolvePackageDependency(name, e, dependencies))
                .ToHashSet();

            if (scripts.ContainsKey("test"))
                return new TestProject(name, version, file, projectDeps, packageDeps, auditRules, mapper, shell, logger);

            return new LibraryProject(name, version, file, projectDeps, packageDeps, auditRules, mapper, shell, logger);
        }
    }
}