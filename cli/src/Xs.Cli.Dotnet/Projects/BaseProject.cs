using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class BaseProject : ProjectBase, ISpecialProject, IAuditableProject, ICachingProject, ICleanableProject, IInstallableProject, IBuildableProject
    {
        private static object cacheLocker = new object();

        public TargetFramework TargetFramework { get; }

        public OutputType OutputType { get; }

        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;

        private readonly ProjectMapper mapper;

        public BaseProject(SpecialProjectContext context) : base(context)
        {
            TargetFramework = context.TargetFramework;
            OutputType = context.OutputType;
            auditRules = context.AuditRules;
            mapper = context.Mapper;
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

        public override void Save() => mapper.Save(this);

        protected override bool IsRelatedDirectory(DirectoryInfo directory) => directory
            .GetFiles("*", SearchOption.AllDirectories)
            .Any(IsRelatedFile);

        protected override bool IsRelatedFile(FileInfo file) =>
            (
                file.FullName.EndsWith(ProjectFactory.ProjectFileExtension) ||
                ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith)
            ) &&
            !ProjectFactory.IgnoredFolders.Any(file.FullName.Contains);
    }
}