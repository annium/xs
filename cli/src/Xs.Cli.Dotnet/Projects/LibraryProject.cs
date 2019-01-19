using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class LibraryProject : ISpecialProject, ICleanableProject, IInstallableProject, IBuildableProject
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public string Name { get; }

        public FileInfo File { get; }

        public TargetFramework TargetFramework { get; }

        public OutputType OutputType { get; }

        public HashSet<IProject> ProjectDependencies { get; }

        public HashSet<Dependency> PackageDependencies { get; }

        protected readonly ILogger logger;

        private readonly ProjectMapper mapper;

        protected readonly IShell shell;

        public LibraryProject(
            string name,
            FileInfo file,
            TargetFramework targetFramework,
            OutputType outputType,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        )
        {
            this.Name = name;
            this.File = file;
            this.TargetFramework = targetFramework;
            this.OutputType = outputType;
            this.ProjectDependencies = projectDependencies;
            this.PackageDependencies = packageDependencies;
            this.mapper = mapper;
            this.shell = shell;
            this.logger = logger;
        }

        public Task CleanAsync(CancellationToken token)
        {
            logger.LogInfo($"Cleaning {Name}");

            DeleteDirectory("bin");
            DeleteDirectory("obj");

            DeleteFiles("*.nupkg");
            DeleteFiles("*.snupkg");

            logger.LogInfo($"Cleaned {Name}");

            return Task.CompletedTask;

            void DeleteDirectory(string path)
            {
                path = Path.Combine(File.DirectoryName, path);
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive : true);
            }

            void DeleteFiles(string mask)
            {
                foreach (var file in Directory.GetFiles(File.DirectoryName, mask, SearchOption.TopDirectoryOnly))
                    System.IO.File.Delete(file);
            }
        }

        public async Task InstallAsync(CancellationToken token)
        {
            logger.LogInfo($"Installing {Name}");

            var result = await shell.RunAsync(
                $"dotnet restore --no-dependencies {File.FullName}",
                token);

            if (result.Code == 0)
                logger.LogInfo($"Installed {Name}");
            else
                throw new Exception($"Failed to install {Name}:{Environment.NewLine}{result.Output}");
        }

        public async Task BuildAsync(
            Env env,
            CancellationToken token
        )
        {
            logger.LogInfo($"Building {Name}");

            var configuration = env == Env.Development ? "Debug" : "Release";
            var result = await shell.RunAsync(
                $"dotnet build --configuration {configuration} --no-dependencies {File.FullName}",
                token);

            if (result.Code == 0)
                logger.LogInfo($"Built {Name}");
            else
                throw new Exception($"Failed to build {Name}:{Environment.NewLine}{result.Output}");
        }

        public bool IsRelated(string path)
        {
            if (!path.StartsWith(File.DirectoryName))
                return false;

            return Directory.Exists(path) ?
                IsRelatedDirectory(new DirectoryInfo(path)) :
                IsRelatedFile(new FileInfo(path));
        }

        public void Save() => mapper.Save(this.File.FullName, this);

        public override string ToString() => Name;

        private bool IsRelatedDirectory(DirectoryInfo directory) => directory
            .GetFiles("*", SearchOption.AllDirectories)
            .Any(IsRelatedFile);

        private bool IsRelatedFile(FileInfo file) =>
            (
                file.FullName.EndsWith(ProjectFactory.ProjectFileExtension) ||
                ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith)
            ) &&
            !ProjectFactory.IgnoredFolders.Any(file.FullName.Contains);
    }
}