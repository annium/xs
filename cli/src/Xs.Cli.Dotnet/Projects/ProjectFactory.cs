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

namespace Xs.Cli.Dotnet.Projects
{
    internal class ProjectFactory : SpecialProjectFactoryBase<ISpecialProject>, ISpecialProjectFactory
    {
        public const string ProjectFileExtension = ".csproj";
        public const string TestCoveragePackage = "coverlet.msbuild";
        public static readonly string[] TrackedFileExtensions = new[] { ".cs" };
        public static readonly string[] IgnoredFolders = new[] { "bin", "obj" };
        private const string ProjectFileMask = "*.csproj";
        public ProjectType Type => Constants.ProjectType;
        private readonly IEnumerable<IAuditRule<ISpecialProject>> _auditRules;
        private readonly ProjectMapper _mapper;
        private readonly IShell _shell;
        private readonly LoggerConfiguration _loggerConfiguration;
        private readonly IServiceProvider _provider;

        public ProjectFactory(
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            LoggerConfiguration loggerConfiguration,
            IServiceProvider provider
        )
        {
            _auditRules = auditRules;
            _mapper = mapper;
            _shell = shell;
            _loggerConfiguration = loggerConfiguration;
            _provider = provider;
        }

        public bool IsProjectDirectory(string directory)
        {
            // considered project directory, if in current directory there's single project file
            // and it's only one in all subdirectories
            return Directory.Exists(directory) &&
                Directory.GetFiles(directory, ProjectFileMask).Length == 1 &&
                !FileManager.FindDirectory(directory, IsMatch, IgnoredFolders);

            static bool IsMatch(string dir) => Directory.GetFiles(dir, ProjectFileMask).Length > 0;
        }

        public bool IsProjectFile(string file)
        {
            if (!file.EndsWith(ProjectFileExtension))
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
            var file = new FileInfo(Directory.GetFiles(directory, ProjectFileMask, SearchOption.TopDirectoryOnly).First());
            var (name, version, description, targetFramework, outputType, projectDeps, packageDeps, isPackable, isTestProject) =
                _mapper.Load(file.FullName, configuration);

            var projectDependencies = projectDeps
                .Select(e => GetProjectDependencyMock(file, e))
                .ToHashSet();

            var packageDependencies = packageDeps.ToHashSet();

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
                    _shell,
                    _loggerConfiguration,
                    _provider.Resolve<ILogger<TProject>>(),
                    targetFramework,
                    outputType,
                    _auditRules,
                    _mapper
                );
        }
    }
}