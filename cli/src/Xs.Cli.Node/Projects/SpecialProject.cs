using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Shell;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Node.Tools;
using SysDirectory = System.IO.Directory;

namespace Xs.Cli.Node.Projects
{
    internal abstract class SpecialProject<TProject> : ProjectBase<TProject>, ISpecialProject, IAuditableProject, ICachingProject, ICleanableProject, IInstallableProject, IBuildableProject where TProject : SpecialProject<TProject>
    {
        // TODO: rewrite through project options - projects can have different shapes in a moment
        private static readonly object cacheLocker = new object();
        private static readonly Lazy<string> cacheDir = new Lazy<string>(valueFactory: ResolveCacheDir, isThreadSafe: true);
        private static IShell? staticShell;

        private static string ResolveCacheDir()
        {
            lock (cacheLocker)
                return staticShell!.Cmd("yarn cache dir").RunAsync().GetAwaiter().GetResult().Output.Trim();
        }

        public override string File => Path.Combine(Directory, ProjectFactory.ProjectFileName);
        protected readonly IReadOnlyDictionary<string, string> scripts;
        private readonly IEnumerable<IAuditRule<ISpecialProject>> auditRules;
        private readonly ProjectMapper mapper;

        public SpecialProject(SpecialProjectContext<TProject> context) : base(context)
        {
            scripts = context.Scripts;
            auditRules = context.AuditRules;
            mapper = context.Mapper;

            // set static shell if not set yet
            lock (cacheLocker) if (staticShell is null) staticShell = context.Shell;
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

            lock (cacheLocker)
            {
                var entries = SysDirectory.GetDirectories(cacheDir.Value);
                foreach (var (_, pkg) in Packages)
                {
                    var name = PackageName.GetPlainName(pkg.Name);
                    var version = pkg.Version.ToString();
                    foreach (var entry in entries.Where(e => e.Contains(name) && e.Contains(version)))
                        if (SysDirectory.Exists(entry))
                            SysDirectory.Delete(entry, recursive: true);
                }
            }

            logger.Info($"Finished {Name} cache clean.");

            return Task.CompletedTask;
        }

        public async Task CleanAsync(bool force, CancellationToken token)
        {
            logger.Info($"Start {Name} clean.");

            DeleteDirectory(ProjectFactory.ModulesDirectory);
            DeleteFiles("*.tgz");
            if (force)
            {
                DeleteFiles(ProjectFactory.LockFileName);
            }

            if (scripts.ContainsKey("clean"))
                await RunAsync("yarn clean", "yarn run clean", token);

            logger.Info($"Finished {Name} clean.");
        }

        public Task InstallAsync(bool force, CancellationToken token)
        {
            if (force)
            {
                DeleteDirectory(ProjectFactory.ModulesDirectory);
                DeleteFiles(ProjectFactory.LockFileName);
            }

            return RunAsync("install", $"yarn install --no-emoji --no-progress", token);
        }

        public Task BuildAsync(Env env, CancellationToken token) =>
        scripts.ContainsKey("build") ? RunAsync("build", "yarn run build", token) : Task.CompletedTask;

        protected override void HandleSave() => mapper.Save(this);

        protected override bool IsRelated(FileInfo file) =>
        ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith) &&
        !FileManager.IsRootedDirectoryIgnored(Directory, file.DirectoryName, ProjectFactory.IgnoredFolders);
    }
}