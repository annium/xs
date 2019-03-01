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

namespace Xs.Cli.Node.Projects
{
    internal class BaseProject : ProjectBase, ISpecialProject, IAuditableProject, ICachingProject, ICleanableProject, IInstallableProject, IBuildableProject
    {
        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;

        private readonly ProjectMapper mapper;

        public BaseProject(
            string name,
            Version version,
            string description,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        ) : base(
            Constants.ProjectType,
            name,
            version,
            description,
            file,
            projectDependencies,
            packageDependencies,
            shell,
            logger
        )
        {
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

        public Task ClearCacheAsync(CancellationToken token) =>
            RunAsync("cache clean", $"yarn cache clean {string.Join(' ',PackageDependencies.Select(d=>d.Name))}", token);

        public Task CleanAsync(CancellationToken token)
        {
            logger.LogInfo($"Start {Name} clean.");

            DeleteDirectory(ProjectFactory.ModulesDirectory);

            DeleteFiles("*.tgz");

            logger.LogInfo($"Finished {Name} clean.");

            return Task.CompletedTask;
        }

        public Task InstallAsync(bool force, CancellationToken token)
        {
            var forceFlag = force ? "--force" : string.Empty;

            return RunAsync("install", $"yarn install {forceFlag} --no-emoji --no-progress", token);
        }

        public Task BuildAsync(Env env, CancellationToken token) =>
            RunAsync("build", "yarn run build", token);

        public override bool IsRelated(string path)
        {
            if (!path.StartsWith(File.DirectoryName))
                return false;

            if (Directory.Exists(path))
                return IsRelatedDirectory(new DirectoryInfo(path));

            return IsRelatedFile(new FileInfo(path));
        }

        public override void Save() => mapper.Save(this);

        private bool IsRelatedDirectory(DirectoryInfo directory)
        {
            return directory.GetFiles("*", SearchOption.AllDirectories).Any(IsRelatedFile);
        }

        private bool IsRelatedFile(FileInfo file) =>
            (
                file.FullName.EndsWith(ProjectFactory.ProjectFileName) ||
                ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith)
            ) &&
            !ProjectFactory.IgnoredFolders.Any(file.FullName.Contains);
    }
}