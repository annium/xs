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
using Xs.Core.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class LibraryProject : ProjectBase, ISpecialProject, ICleanableProject, IInstallableProject, IBuildableProject, IPublishableProject
    {
        private static object cacheLocker = new object();

        public override ProjectType Type { get; } = Constants.ProjectType;

        public override string Name { get; }

        public override FileInfo File { get; }

        public override HashSet<IProject> ProjectDependencies { get; }

        public override HashSet<Dependency> PackageDependencies { get; }

        public TargetFramework TargetFramework { get; }

        public OutputType OutputType { get; }

        protected readonly ILogger logger;

        private readonly ProjectMapper mapper;

        protected readonly IShell shell;

        public LibraryProject(
            string name,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger,
            TargetFramework targetFramework,
            OutputType outputType
        ) : base(shell, logger)
        {
            this.Name = name;
            this.File = file;
            this.ProjectDependencies = projectDependencies;
            this.PackageDependencies = packageDependencies;
            this.TargetFramework = targetFramework;
            this.OutputType = outputType;
            this.mapper = mapper;
            this.shell = shell;
            this.logger = logger;
        }

        public Task CleanAsync(CancellationToken token)
        {
            logger.LogInfo($"Start {Name} clean.");

            DeleteDirectory("bin");
            DeleteDirectory("obj");

            DeleteFiles("*.nupkg");
            DeleteFiles("*.snupkg");

            logger.LogInfo($"Finished {Name} clean.");

            return Task.CompletedTask;
        }

        public Task InstallAsync(bool force, CancellationToken token)
        {
            if (!force)
                return RunAsync("install", $"dotnet restore --no-dependencies {File.FullName}", token);

            var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            foreach (var(_, name, version) in PackageDependencies)
            {
                var cachePath = Path.Combine(cache, name.ToLowerInvariant(), version.ToString());
                lock(cacheLocker)
                {
                    if (Directory.Exists(cachePath))
                        Directory.Delete(cachePath, recursive : true);
                }
            }

            return RunAsync("install", $"dotnet restore --no-cache --no-dependencies {File.FullName}", token);
        }

        public Task BuildAsync(Env env, CancellationToken token)
        {
            var configuration = env == Env.Development ? "Debug" : "Release";

            return RunAsync(
                "build",
                $"dotnet build --configuration {configuration} --no-dependencies {File.FullName}",
                token);
        }

        public async Task<string> PackAsync(Core.Models.Version version, CancellationToken token)
        {
            var file = Path.Combine(File.DirectoryName, $"{Name}.{version}.nupkg");
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);

            await RunAsync(
                "pack",
                $"dotnet pack {File.FullName} --output . -p:PackageVersion={version} -p:SymbolPackageFormat=snupkg",
                token);

            return file;
        }

        public async Task PublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token)
        {
            var packageFile = await PackAsync(version, token);

            await RunAsync(
                "publish",
                $"dotnet nuget push {packageFile} --source {registry} --api-key {accessToken}",
                token);

            System.IO.File.Delete(packageFile);
        }

        public Task UnpublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token) =>
            RunAsync(
                "unpublish",
                $"dotnet nuget delete {Name} {version} --source {registry} --api-key {accessToken} --non-interactive",
                token);

        public override bool IsRelated(string path)
        {
            if (!path.StartsWith(File.DirectoryName))
                return false;

            return Directory.Exists(path) ?
                IsRelatedDirectory(new DirectoryInfo(path)) :
                IsRelatedFile(new FileInfo(path));
        }

        public override void Save() => mapper.Save(this);

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