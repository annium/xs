using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives.Collections.Generic;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using LibGit2Sharp;
using ConsoleExt = Annium.Extensions.CommandLine.Cli;

namespace Xs.Commands.Sync;

internal class SyncCommand : AsyncCommand<SyncCommandConfiguration>
{
    public override string Id => "";
    public override string Description => "Execute repositories sync";
    private static readonly IReadOnlyCollection<FileStatus> Statuses = Enum.GetValues<FileStatus>().Except(FileStatus.Unaltered.Yield()).ToArray();
    private static readonly IReadOnlyDictionary<FileStatus, string> StatusLabels;
    private string Indent => new(' ', _indentation);
    private readonly SyncConfigurator _configurator;
    private readonly IShell _shell;
    private int _indentation;

    static SyncCommand()
    {
        var statuses = new Dictionary<FileStatus, string>();
        statuses[FileStatus.Nonexistent] = "missing";
        statuses[FileStatus.Unaltered] = "unchanged";
        statuses[FileStatus.NewInIndex] = "new";
        statuses[FileStatus.ModifiedInIndex] = "modified";
        statuses[FileStatus.DeletedFromIndex] = "deleted";
        statuses[FileStatus.RenamedInIndex] = "renamed";
        statuses[FileStatus.TypeChangeInIndex] = "modified";
        statuses[FileStatus.NewInWorkdir] = "new";
        statuses[FileStatus.ModifiedInWorkdir] = "modified";
        statuses[FileStatus.DeletedFromWorkdir] = "deleted";
        statuses[FileStatus.TypeChangeInWorkdir] = "modified";
        statuses[FileStatus.RenamedInWorkdir] = "renamed";
        statuses[FileStatus.Unreadable] = "unreadable";
        statuses[FileStatus.Ignored] = "ignored";
        statuses[FileStatus.Conflicted] = "conflict";
        StatusLabels = statuses;
    }

    public SyncCommand(
        SyncConfigurator configurator,
        IShell shell
    )
    {
        _configurator = configurator;
        _shell = shell;
    }

    public override async Task HandleAsync(
        SyncCommandConfiguration cfg,
        CancellationToken ct
    )
    {
        var projects = _configurator.Read();

        if (cfg.Path == string.Empty)
        {
            Line($"sync {projects.Count} project(s)");

            foreach (var project in projects)
                await SyncProject(project);
        }
        else
        {
            var path = Path.GetFullPath(cfg.Path.TrimEnd('/'));
            var project = projects.SingleOrDefault(x => x.Path == path) ?? Sync.SyncProject.CreateDefault(path);
            await SyncProject(project);
        }
    }

    private async Task SyncProject(SyncProject project)
    {
        Line($"{project.Path}:");

        AddIndent();

        using var repo = new Repository(project.Path);

        var localBranches = repo.Branches.Where(x => !x.IsRemote).ToArray();

        var changes = repo.RetrieveStatus()
            .Where(x => x.State is not FileStatus.Ignored)
            .ToArray();
        var touchedPaths = changes
            .SelectMany(x => x.State switch
            {
                FileStatus.RenamedInIndex   => new[] { x.HeadToIndexRenameDetails.OldFilePath, x.HeadToIndexRenameDetails.NewFilePath },
                FileStatus.RenamedInWorkdir => new[] { x.IndexToWorkDirRenameDetails.OldFilePath, x.IndexToWorkDirRenameDetails.NewFilePath },
                _                           => x.FilePath.Yield()
            })
            .ToHashSet();

        foreach (var remote in repo.Network.Remotes)
            await SyncRemote(project, repo, remote, localBranches, touchedPaths);

        if (changes.Length > 0)
        {
            using var _ = ConsoleExt.SetColors(foreground: ConsoleColor.Magenta);
            Line("changes in working directory:");
            foreach (var change in changes)
            {
                var fileChange = change.State switch
                {
                    FileStatus.RenamedInIndex   => $"{change.HeadToIndexRenameDetails.OldFilePath} -> {change.HeadToIndexRenameDetails.NewFilePath}",
                    FileStatus.RenamedInWorkdir => $"{change.IndexToWorkDirRenameDetails.OldFilePath} -> {change.IndexToWorkDirRenameDetails.NewFilePath}",
                    _                           => change.FilePath
                };
                var status = Statuses.Where(x => change.State.HasFlag(x)).Select(x => StatusLabels[x]).Distinct().Join(", ");
                Line($"  {status}: {fileChange}");
            }
        }

        RemoveIndent();
    }

    private async Task SyncRemote(
        SyncProject project,
        Repository repo,
        LibGit2Sharp.Remote remote,
        IReadOnlyCollection<Branch> localBranches,
        IReadOnlyCollection<string> touchedPaths
    )
    {
        Line($"{remote.Name}:");

        AddIndent();

        Pending("fetch - ");
        await _shell
            .Cmd($"git fetch {remote.Name} -p")
            .At(repo.Info.WorkingDirectory)
            .ExecuteAsync();
        Success("done");

        var remoteBranches = repo.Branches.Where(x => x.IsRemote && x.RemoteName == remote.Name).ToArray();
        foreach (var localBranch in localBranches)
        {
            var remoteBranch = remoteBranches.SingleOrDefault(x => x.UpstreamBranchCanonicalName == localBranch.CanonicalName);
            await SyncBranch(project, repo, remote, localBranch, remoteBranch, touchedPaths);
        }

        RemoveIndent();
    }

    private async Task SyncBranch(
        SyncProject project,
        Repository repo,
        LibGit2Sharp.Remote remote,
        Branch localBranch,
        Branch? remoteBranch,
        IReadOnlyCollection<string> touchedPaths
    )
    {
        Pending($"{localBranch.CanonicalName} - ");
        if (remoteBranch is null)
        {
            if (project.Config.Push)
                await Push(repo, remote, localBranch);
            else
                Info("kept local");
            return;
        }

        var localCommits = localBranch.Commits.ToList();
        var remoteCommits = remoteBranch.Commits.ToList();

        if (localCommits.Count == 0)
        {
            Warning("no local commits");
            return;
        }

        if (remoteCommits.Count == 0)
        {
            Warning("no remote commits");
            return;
        }

        var localHead = localCommits[0];
        var remoteHead = remoteCommits[0];
        if (localHead.Id == remoteHead.Id)
        {
            Info("up to date");
            return;
        }

        // if push needed
        if (localCommits.Contains(remoteHead))
        {
            await Push(repo, remote, localBranch);
            return;
        }

        // if pull needed
        if (remoteCommits.Contains(localHead))
        {
            await Pull(repo, remote, localBranch, localHead, remoteHead, touchedPaths);
            return;
        }

        DescribeDivergence(localCommits, remoteCommits);
    }

    private async Task Pull(
        Repository repo,
        LibGit2Sharp.Remote remote,
        Branch localBranch,
        Commit localHead,
        Commit remoteHead,
        IReadOnlyCollection<string> touchedPaths
    )
    {
        var diff = repo.Diff.Compare<TreeChanges>(localHead.Tree, remoteHead.Tree);
        var hasIntersections = diff.SelectMany(x => new[] { x.OldPath, x.Path }).Intersect(touchedPaths).Any();
        if (hasIntersections)
        {
            Warning("skipped - local changes will be affected by pull");
            return;
        }

        await _shell.Cmd($"git pull {remote.Name} {localBranch.CanonicalName}").At(repo.Info.WorkingDirectory).ExecuteAsync();
        Success("pulled");
    }

    private async Task Push(
        Repository repo,
        LibGit2Sharp.Remote remote,
        Branch localBranch
    )
    {
        await _shell.Cmd($"git push {remote.Name} {localBranch.CanonicalName}").At(repo.Info.WorkingDirectory).ExecuteAsync();
        Success("pushed");
    }

    private void DescribeDivergence(
        List<Commit> localCommits,
        List<Commit> remoteCommits
    )
    {
        // branches diverged - find common ancestor commit
        var localIndex = -1;
        var remoteIndex = -1;
        for (var i = 0; i < localCommits.Count; i++)
        {
            var localCommit = localCommits[i];
            remoteIndex = remoteCommits.FindIndex(x => x.Id == localCommit.Id);
            if (remoteIndex == -1)
                continue;

            localIndex = i;
            break;
        }

        Warning(localIndex == -1 ? "local and remote branches have completely diverged" : $"local and remote branches have diverged by {localIndex} and {remoteIndex} commit(s) respectively");
    }

    private void Pending(string message) => Console.Write($"{Indent}{message}");
    private void Line(string message) => Console.WriteLine($"{Indent}{message}");
    private void Info(string message) => ConsoleExt.WriteLineColored(message, foreground: ConsoleColor.Blue);
    private void Success(string message) => ConsoleExt.WriteLineColored(message, foreground: ConsoleColor.Green);
    private void Error(string message) => ConsoleExt.WriteLineColored(message, foreground: ConsoleColor.Red);
    private void Warning(string message) => ConsoleExt.WriteLineColored(message, foreground: ConsoleColor.Yellow);
    private void AddIndent() => _indentation += 2;
    private void RemoveIndent() => _indentation -= 2;
}

internal class SyncCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Repository path.")]
    public string Path { get; set; } = string.Empty;
}

file static class ShellInstanceExtensions
{
    public static async Task ExecuteAsync(this IShellInstance shell)
    {
        var result = await shell.RunAsync();
        if (result.IsSuccess)
            return;

        var command = string.Empty;
        shell.Configure(info => command = $"{info.FileName} {info.Arguments}");
        throw new Exception($"{command} failed:{Environment.NewLine}{result.Error}");
    }
}