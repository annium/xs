using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class LibraryProject : ProjectBase, ISpecialProject, IAuditableProject, ICachingProject, ICleanableProject, IInstallableProject, IBuildableProject, IPublishableProject
    {
        private static object cacheLocker = new object();

        public TargetFramework TargetFramework { get; }

        public OutputType OutputType { get; }

        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;

        private readonly ProjectMapper mapper;

        public LibraryProject(
            string name,
            Core.Models.Version version,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            TargetFramework targetFramework,
            OutputType outputType,
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        ) : base(
            Constants.ProjectType,
            name,
            version,
            file,
            projectDependencies,
            packageDependencies,
            shell,
            logger
        )
        {
            TargetFramework = targetFramework;
            OutputType = outputType;
            this.auditRules = auditRules;
            this.mapper = mapper;
        }

        public AuditResult[] Audit(bool fix, CancellationToken token)
        {
            var results = new List<AuditResult>();

            foreach (var rule in auditRules)
                results.AddRange(rule.Execute(this, fix));

            return results.ToArray();
        }

        public Task ClearCacheAsync(CancellationToken token)
        {
            logger.LogInfo($"Start {Name} cache clean.");

            var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            lock(cacheLocker)
            {
                foreach (var(_, name, version) in PackageDependencies)
                {
                    var cachePath = Path.Combine(cache, name.ToLowerInvariant(), version.ToString());
                    if (Directory.Exists(cachePath))
                        Directory.Delete(cachePath, recursive : true);
                }
            }

            logger.LogInfo($"Finished {Name} cache clean.");

            return Task.CompletedTask;
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
            var forceFlag = force ? "--no-cache" : string.Empty;

            return RunAsync("install", $"dotnet restore {forceFlag} --no-dependencies {File.FullName}", token);
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

            Version = version;
            Save();

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
                $"dotnet nuget push {packageFile} --source {new Uri(registry, Constants.ServerPathSuffix)} --api-key {accessToken}",
                token);

            System.IO.File.Delete(packageFile);
        }

        public Task UnpublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken token) =>
            RunAsync(
                "unpublish",
                $"dotnet nuget delete {Name} {version} --source {new Uri(registry, Constants.ServerPathSuffix)} --api-key {accessToken} --non-interactive",
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