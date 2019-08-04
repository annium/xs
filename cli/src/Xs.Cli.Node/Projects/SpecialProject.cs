using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Node.Tools;

namespace Xs.Cli.Node.Projects
{
    internal abstract class SpecialProject<TProject> : ProjectBase<TProject>, ISpecialProject, IAuditableProject, ICachingProject, ICleanableProject, IInstallableProject, IBuildableProject where TProject : SpecialProject<TProject>
    {
        private static string cacheDir;
        private static object cacheLocker = new object();
        protected readonly IReadOnlyDictionary<string, string> scripts;
        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;
        private readonly ProjectMapper mapper;

        public SpecialProject(SpecialProjectContext<TProject> context) : base(context)
        {
            scripts = context.Scripts;
            auditRules = context.AuditRules;
            mapper = context.Mapper;

            lock(cacheLocker)
            {
                if (string.IsNullOrEmpty(cacheDir))
                {
                    var result = context.Shell.Cmd("yarn cache dir").RunAsync().GetAwaiter().GetResult();
                    cacheDir = result.Output.Trim();
                }
            }
        }

        public AuditResult[] Audit(IProject[] projects, string[] rules, bool fix, CancellationToken token)
        {
            var results = new List<AuditResult>();

            foreach (var rule in auditRules.Where(r => rules.Contains(r.Code)))
                results.AddRange(rule.Execute(projects, this, fix));

            return results.ToArray();
        }

        public Task ClearCacheAsync(CancellationToken token)
        {
            logger.Info($"Start {Name} cache clean.");

            lock(cacheLocker)
            {
                var entries = Directory.GetDirectories(cacheDir);
                foreach (var(_, pkg) in Packages)
                {
                    var name = PackageName.GetPlainName(pkg.Name);
                    var version = pkg.Version.ToString();
                    foreach (var entry in entries.Where(e => e.Contains(name) && e.Contains(version)))
                        if (Directory.Exists(entry))
                            Directory.Delete(entry, recursive : true);
                }
            }

            logger.Info($"Finished {Name} cache clean.");

            return Task.CompletedTask;
        }

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
            DeleteDirectory(ProjectFactory.ModulesDirectory);
            DeleteFiles(ProjectFactory.LockFileName);

            return RunAsync("install", $"yarn install --no-emoji --no-progress", token);
        }

        public Task BuildAsync(Env env, CancellationToken token) =>
        scripts.ContainsKey("build") ? RunAsync("build", "yarn run build", token) : Task.CompletedTask;

        public override void Save() => mapper.Save(this);

        protected override bool IsRelated(FileInfo file) =>
        ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith) &&
        !FileManager.IsRootedDirectoryIgnored(File.DirectoryName, file.DirectoryName, ProjectFactory.IgnoredFolders);
    }
}