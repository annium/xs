using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class BaseProject : ProjectBase, ISpecialProject, IAuditableProject, ICachingProject, ICleanableProject, IInstallableProject, IBuildableProject
    {
        protected readonly IReadOnlyDictionary<string, string> scripts;

        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;

        private readonly ProjectMapper mapper;

        public BaseProject(SpecialProjectContext context) : base(context)
        {
            scripts = context.Scripts;
            auditRules = context.AuditRules;
            mapper = context.Mapper;
        }

        public AuditResult[] Audit(IProject[] projects, string[] rules, bool fix, CancellationToken token)
        {
            var results = new List<AuditResult>();

            foreach (var rule in auditRules.Where(r => rules.Contains(r.Code)))
                results.AddRange(rule.Execute(projects, this, fix));

            return results.ToArray();
        }

        public Task ClearCacheAsync(CancellationToken token) =>
            RunAsync("cache clean", $"yarn cache clean {string.Join(' ',PackageDependencies.Select(d=>d.Name))}", token);

        public Task CleanAsync(CancellationToken token)
        {
            logger.Info($"Start {Name} clean.");

            DeleteDirectory(ProjectFactory.ModulesDirectory);

            DeleteFiles("*.tgz");

            logger.Info($"Finished {Name} clean.");

            return Task.CompletedTask;
        }

        public Task InstallAsync(bool force, CancellationToken token)
        {
            var forceFlag = force ? "--force" : string.Empty;

            return RunAsync("install", $"yarn install {forceFlag} --no-emoji --no-progress", token);
        }

        public Task BuildAsync(Env env, CancellationToken token) =>
            scripts.ContainsKey("build") ? RunAsync("build", "yarn run build", token) : Task.CompletedTask;

        public override void Save() => mapper.Save(this);

        protected override bool IsRelated(FileInfo file) =>
            ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith) &&
            !FileManager.IsRootedDirectoryIgnored(File.DirectoryName, file.DirectoryName, ProjectFactory.IgnoredFolders);
    }
}