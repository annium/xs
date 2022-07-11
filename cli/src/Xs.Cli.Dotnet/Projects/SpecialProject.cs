using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Models;
using SpecialConfiguration = Xs.Cli.Dotnet.Tools.SpecialConfiguration;
using SysDirectory = System.IO.Directory;
using SysFile = System.IO.File;

namespace Xs.Cli.Dotnet.Projects;

internal abstract class SpecialProject<TProject> :
    ProjectBase<TProject>,
    ISpecialProject,
    IAuditableProject,
    ICachingProject,
    ICleanableProject,
    IInstallableProject,
    IBuildableProject
    where TProject : SpecialProject<TProject>
{
    private static readonly object CacheLocker = new();
    public override string File => Path.Combine(Directory, ProjectFileName(Name));
    public TargetFramework TargetFramework { get; }
    public OutputType OutputType { get; }
    public SpecialConfiguration Config { get; set; }
    private readonly IEnumerable<IAuditRule<ISpecialProject>> _auditRules;
    private readonly ProjectMapper _mapper;

    protected SpecialProject(SpecialProjectContext<TProject> context) : base(context)
    {
        Config = context.Config;
        TargetFramework = context.TargetFramework;
        OutputType = context.OutputType;
        _auditRules = context.AuditRules;
        _mapper = context.Mapper;
    }

    public AuditResult[] Audit(IProject[] projects, string[] rules, bool fix, CancellationToken ct)
    {
        var results = new List<AuditResult>();

        foreach (var rule in _auditRules.Where(r => rules.Contains(r.Code)))
            results.AddRange(rule.Execute(projects, this, fix));

        return results.ToArray();
    }

    public Task ClearCacheAsync(CancellationToken ct)
    {
        this.Log().Info($"Start {Name} cache clean.");

        var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        lock (CacheLocker)
        {
            foreach (var (_, (_, name, version)) in Packages)
            {
                var cachePath = Path.Combine(cache, name.ToLowerInvariant(), version.ToString());
                if (SysDirectory.Exists(cachePath))
                    SysDirectory.Delete(cachePath, recursive: true);
            }
        }

        this.Log().Info($"Finished {Name} cache clean.");

        return Task.CompletedTask;
    }

    public Task CleanAsync(bool force, CancellationToken ct)
    {
        this.Log().Info($"Start {Name} clean.");

        DeleteDirectory("bin");
        DeleteDirectory("obj");

        DeleteFiles("*.nupkg");
        DeleteFiles("*.snupkg");

        this.Log().Info($"Finished {Name} clean.");

        return Task.CompletedTask;
    }

    public Task InstallAsync(bool force, CancellationToken ct)
    {
        var forceFlag = force ? "--no-cache" : string.Empty;

        return RunAsync("install", $"dotnet restore {forceFlag} --no-dependencies {File}", ct);
    }

    public Task BuildAsync(Env env, bool force, CancellationToken ct)
    {
        var configuration = env == Env.Development ? "Debug" : "Release";

        if (force)
            DeleteDirectory("bin");

        return RunAsync(
            "build",
            $"dotnet build --configuration {configuration} --no-dependencies {File}",
            ct);
    }

    protected override void HandleSave() => _mapper.Save(this);

    protected override string FixProjectDirectory(string directory)
    {
        return Path.Combine(Path.GetDirectoryName(directory)!, Name);
    }

    protected override void OnNameChangeSave(string oldName, string newName)
    {
        var oldPath = Path.Combine(Directory, ProjectFileName(oldName));
        var newPath = Path.Combine(Directory, ProjectFileName(newName));
        SysFile.Move(oldPath, newPath);
    }

    protected override bool IsRelated(FileInfo file) =>
        ProjectFactory.TrackedFileExtensions.Any(file.FullName.EndsWith) &&
        !FileManager.IsRootedDirectoryIgnored(Directory, file.DirectoryName!, ProjectFactory.IgnoredFolders);

    private string ProjectFileName(string name) => $"{name}{ProjectFactory.ProjectFileExtension}";
}