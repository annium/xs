using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Xx.Cli.Core.Audit;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;
using Xx.Cli.Core.Tools;
using PlatformConfiguration = Xx.Cli.Node.Tools.PlatformConfiguration;

namespace Xx.Cli.Node.Projects;

internal abstract class PlatformProject
    : ProjectBase,
        IPlatformProject,
        IAuditableProject,
        ICachingProject,
        ICleanableProject,
        IInstallableProject,
        IBuildableProject
{
    // TODO: rewrite through project options - projects can have different shapes in a moment
    // private static readonly object CacheLocker = new object();
    // private static readonly Lazy<string> CacheDir = new Lazy<string>(valueFactory: ResolveCacheDir, isThreadSafe: true);
    // private static IShell? _StaticShell;

    // private static string ResolveCacheDir()
    // {
    //     lock (CacheLocker)
    //         return _StaticShell!.Cmd("yarn cache dir").RunAsync().GetAwaiter().GetResult().Output.Trim();
    // }

    public PlatformConfiguration Config { get; set; }
    public override string File => Path.Combine(Directory, ProjectFactory.ProjectFileName);
    protected readonly IReadOnlyDictionary<string, string> Scripts;
    private readonly IEnumerable<IAuditRule<IPlatformProject>> _auditRules;
    private readonly ProjectMapper _mapper;

    public PlatformProject(PlatformProjectContext context)
        : base(context)
    {
        Config = context.Config;
        Scripts = context.Scripts;
        _auditRules = context.AuditRules;
        _mapper = context.Mapper;

        // set static shell if not set yet
        // lock (CacheLocker)
        //     if (_StaticShell is null)
        //         _StaticShell = context.Shell;
    }

    public IReadOnlyCollection<AuditResult> Audit(
        IReadOnlyCollection<IProject> projects,
        string[] rules,
        bool fix,
        CancellationToken ct
    )
    {
        var results = new List<AuditResult>();

        foreach (var rule in _auditRules.Where(r => rules.Contains(r.Code)))
            results.AddRange(rule.Execute(projects, this, fix));

        return results.ToArray();
    }

    public Task ClearCacheAsync(CancellationToken ct)
    {
        this.Debug($"Start {Name} cache clean.");

        // lock (CacheLocker)
        // {
        //     var entries = SysDirectory.GetDirectories(CacheDir.Value);
        //     foreach (var (_, pkg) in Packages)
        //     {
        //         var name = PackageName.GetPlainName(pkg.Name);
        //         var version = pkg.Version.ToString();
        //         foreach (var entry in entries.Where(e => e.Contains(name) && e.Contains(version)))
        //             if (SysDirectory.Exists(entry))
        //                 SysDirectory.Delete(entry, recursive: true);
        //     }
        // }

        this.Debug($"Finished {Name} cache clean.");

        return Task.CompletedTask;
    }

    public async Task CleanAsync(bool force, CancellationToken ct)
    {
        this.Debug($"Start {Name} clean.");

        DeleteDirectory(ProjectFactory.ModulesDirectory);
        DeleteFiles("*.tgz");
        if (force)
        {
            DeleteFiles(ProjectFactory.LockFileName);
        }

        if (Scripts.ContainsKey("clean"))
            await RunAsync("pnpm clean", "pnpm run clean", ct);

        this.Debug($"Finished {Name} clean.");
    }

    public Task InstallAsync(bool force, CancellationToken ct)
    {
        if (force)
        {
            DeleteDirectory(ProjectFactory.ModulesDirectory);
            DeleteFiles(ProjectFactory.LockFileName);
        }

        return RunAsync("install", $"pnpm install --silent", ct);
    }

    public async Task BuildAsync(Env env, bool force, CancellationToken ct)
    {
        if (force)
        {
            if (Scripts.ContainsKey("clean"))
                await RunAsync("pnpm clean", "pnpm run clean", ct);

            await InstallAsync(true, ct);
        }

        if (Scripts.ContainsKey("build"))
            await RunAsync("build", "pnpm run build", ct);
    }

    protected override void HandleSave() => _mapper.Save(this);

    protected override bool IsRelated(FileInfo file) =>
        ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith)
        && !FileManager.IsRootedDirectoryIgnored(Directory, file.DirectoryName!, ProjectFactory.IgnoredFolders);
}
