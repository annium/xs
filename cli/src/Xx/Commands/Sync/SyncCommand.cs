using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Linq;
using LibGit2Sharp;
using Xx.Cli.Core.Helpers;
using ConsoleExt = Annium.Extensions.CommandLine.Cli;

namespace Xx.Commands.Sync;

internal class SyncCommand : AsyncCommand<SyncCommand.SyncCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "";
    public static string Description => "Execute repositories sync";
    private readonly SyncConfigurator _configurator;
    private readonly Synchronizer _synchronizer;

    public SyncCommand(SyncConfigurator configurator, IShell shell)
    {
        _configurator = configurator;
        _synchronizer = new Synchronizer(shell);
    }

    public override async Task HandleAsync(SyncCommandConfiguration cfg, CancellationToken ct)
    {
        var projects = ResolveProjects(cfg);

        if (projects.Count == 0)
        {
            Console.WriteLine("no project to sync");
            return;
        }

        Console.WriteLine($"sync {projects.Count} project(s)");
        var states = new ConcurrentBag<SyncProjectState>();
        try
        {
            await Task.WhenAll(
                projects.Select(async project =>
                {
                    states.Add(await _synchronizer.SyncProjectAsync(project));
                    Console.Write('.');
                })
            );
            Console.WriteLine();
        }
        catch (SyncException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Sync {ex.Path} failed:");
            throw ex.Exception;
        }

        var visualizer = new Visualizer();
        foreach (var state in states.OrderBy(x => x.Path))
            visualizer.Show(state);
    }

    private IReadOnlyCollection<SyncProject> ResolveProjects(SyncCommandConfiguration cfg)
    {
        var projects = _configurator.Read();

        if (cfg.PathOrGroup == string.Empty)
            return projects;

        var groups = projects.GroupBy(x => x.Group).ToDictionary(x => x.Key);

        if (groups.ContainsKey(cfg.PathOrGroup))
        {
            var group = cfg.PathOrGroup;
            return projects.Where(x => x.Group == group).ToList();
        }

        var path = Path.GetFullPath(cfg.PathOrGroup.TrimEnd('/'));
        var project = projects.SingleOrDefault(x => x.Path == path);

        return project is null ? Array.Empty<SyncProject>() : new[] { project };
    }

    internal class SyncCommandConfiguration
    {
        [Position(1, isRequired: false)]
        [Help("Project repository path or group name.")]
        public string PathOrGroup { get; set; } = string.Empty;
    }

    private class Synchronizer(IShell shell)
    {
        public async Task<SyncProjectState> SyncProjectAsync(SyncProject project)
        {
            try
            {
                using var repo = new Repository(project.Path);

                var localBranches = repo.Branches.Where(x => !x.IsRemote).ToArray();

                var changes = repo.RetrieveStatus().Where(x => x.State is not FileStatus.Ignored).ToArray();
                var touchedPaths = changes
                    .SelectMany(x =>
                        x.State switch
                        {
                            FileStatus.RenamedInIndex =>
                            [
                                x.HeadToIndexRenameDetails.OldFilePath,
                                x.HeadToIndexRenameDetails.NewFilePath
                            ],
                            FileStatus.RenamedInWorkdir =>
                            [
                                x.IndexToWorkDirRenameDetails.OldFilePath,
                                x.IndexToWorkDirRenameDetails.NewFilePath
                            ],
                            _ => x.FilePath.Yield(),
                        }
                    )
                    .ToHashSet();

                var remoteStates = new List<SyncRemoteState>();
                foreach (var remote in repo.Network.Remotes)
                    remoteStates.Add(await SyncRemoteAsync(project, repo, remote, localBranches, touchedPaths));

                var changeStates = Helper.GetProjectChanges(project);

                return new SyncProjectState(project.Path, remoteStates, changeStates);
            }
            catch (Exception exception)
            {
                throw new SyncException(project.Path, exception);
            }
        }

        private async Task<SyncRemoteState> SyncRemoteAsync(
            SyncProject project,
            Repository repo,
            LibGit2Sharp.Remote remote,
            IReadOnlyCollection<Branch> localBranches,
            IReadOnlyCollection<string> touchedPaths
        )
        {
            await shell.Cmd($"git fetch {remote.Name} -p").At(repo.Info.WorkingDirectory).ExecuteAsync();

            var remoteBranches = repo.Branches.Where(x => x.IsRemote && x.RemoteName == remote.Name).ToArray();
            var branchStates = new List<SyncBranchState>();

            foreach (var localBranch in localBranches)
            {
                var remoteBranch = remoteBranches.SingleOrDefault(x =>
                    x.UpstreamBranchCanonicalName == localBranch.CanonicalName
                );
                branchStates.Add(await SyncBranchAsync(project, repo, remote, localBranch, remoteBranch, touchedPaths));
            }

            return new SyncRemoteState(remote.Name, branchStates);
        }

        private async Task<SyncBranchState> SyncBranchAsync(
            SyncProject project,
            Repository repo,
            LibGit2Sharp.Remote remote,
            Branch localBranch,
            Branch? remoteBranch,
            IReadOnlyCollection<string> touchedPaths
        )
        {
            var name = localBranch.FriendlyName;
            if (remoteBranch is null)
            {
                if (!project.Config.Push)
                    return new SyncBranchState(name, SyncBranchStatus.KeptLocal);

                await PushAsync(repo, remote, localBranch);

                return new SyncBranchState(name, SyncBranchStatus.Pushed);
            }

            var localCommits = localBranch.Commits.ToList();
            var remoteCommits = remoteBranch.Commits.ToList();

            if (localCommits.Count == 0)
                return new SyncBranchState(name, SyncBranchStatus.NoLocalCommits);

            if (remoteCommits.Count == 0)
                return new SyncBranchState(name, SyncBranchStatus.NoRemoteCommits);

            var localHead = localCommits[0];
            var remoteHead = remoteCommits[0];
            if (localHead.Id == remoteHead.Id)
                return new SyncBranchState(name, SyncBranchStatus.UpToDate);

            // if push needed
            if (localCommits.Contains(remoteHead))
            {
                await PushAsync(repo, remote, localBranch);
                return new SyncBranchState(name, SyncBranchStatus.Pushed);
            }

            // if pull needed
            if (remoteCommits.Contains(localHead))
            {
                var status = await PullAsync(repo, remote, localBranch, localHead, remoteHead, touchedPaths);
                return new SyncBranchState(name, status);
            }

            return new SyncBranchState(
                name,
                SyncBranchStatus.Diverged,
                DescribeDivergence(localCommits, remoteCommits)
            );
        }

        private async Task<SyncBranchStatus> PullAsync(
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
                return SyncBranchStatus.PullAvoided;

            await shell
                .Cmd($"git pull {remote.Name} {localBranch.CanonicalName}")
                .At(repo.Info.WorkingDirectory)
                .ExecuteAsync();

            return SyncBranchStatus.Pulled;
        }

        private async Task PushAsync(Repository repo, LibGit2Sharp.Remote remote, Branch localBranch)
        {
            await shell
                .Cmd($"git push {remote.Name} {localBranch.CanonicalName}")
                .At(repo.Info.WorkingDirectory)
                .ExecuteAsync();
        }

        private string DescribeDivergence(List<Commit> localCommits, List<Commit> remoteCommits)
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

            return localIndex == -1
                ? "local and remote branches have completely diverged"
                : $"local and remote branches have diverged by {localIndex} and {remoteIndex} commit(s) respectively";
        }
    }

    private sealed record SyncProjectState(
        string Path,
        IReadOnlyCollection<SyncRemoteState> Remotes,
        IReadOnlyCollection<SyncFileChange> Changes
    );

    private sealed record SyncRemoteState(string Name, IReadOnlyCollection<SyncBranchState> Branches);

    private sealed record SyncBranchState(string Name, SyncBranchStatus Status, string? Description = null);

    private enum SyncBranchStatus
    {
        KeptLocal,
        UpToDate,
        NoLocalCommits,
        NoRemoteCommits,
        Pushed,
        Pulled,
        PullAvoided,
        Diverged,
    }

    private class Visualizer
    {
        private string Indent => new(' ', _indentation);
        private int _indentation;

        public void Show(SyncProjectState state)
        {
            Line($"{state.Path}:");

            AddIndent();
            foreach (var remoteState in state.Remotes)
                ShowRemote(remoteState);
            RemoveIndent();

            if (state.Changes.Count == 0)
                return;

            using var _ = ConsoleExt.SetColors(foreground: ConsoleColor.Magenta);
            Line("changes in working directory:");
            foreach (var change in state.Changes)
                Line($"  {change.Status}: {change.Description}");
        }

        private void ShowRemote(SyncRemoteState state)
        {
            Pending($"{state.Name}: ");
            Info("fetched");

            AddIndent();
            foreach (var branchState in state.Branches)
                ShowBranch(branchState);
            RemoveIndent();
        }

        private void ShowBranch(SyncBranchState state)
        {
            Pending($"{state.Name}: ");
            switch (state.Status)
            {
                case SyncBranchStatus.KeptLocal:
                    Info("kept local");
                    break;
                case SyncBranchStatus.UpToDate:
                    Info("up to date");
                    break;
                case SyncBranchStatus.NoLocalCommits:
                    Warning("no local commits");
                    break;
                case SyncBranchStatus.NoRemoteCommits:
                    Warning("no remote commits");
                    break;
                case SyncBranchStatus.Pushed:
                    Success("pushed");
                    break;
                case SyncBranchStatus.Pulled:
                    Success("pulled");
                    break;
                case SyncBranchStatus.PullAvoided:
                    Warning("skipped - local changes will be affected by pull");
                    break;
                case SyncBranchStatus.Diverged:
                    Warning(state.Description.NotNull());
                    break;
            }
        }

        private void Pending(string message) => Console.Write($"{Indent}{message}");

        private void Line(string message) => Console.WriteLine($"{Indent}{message}");

        private void Info(string message) => ConsoleExt.WriteLineColored(message, foreground: ConsoleColor.Blue);

        private void Success(string message) => ConsoleExt.WriteLineColored(message, foreground: ConsoleColor.Green);

        private void Warning(string message) => ConsoleExt.WriteLineColored(message, foreground: ConsoleColor.Yellow);

        private void AddIndent() => _indentation += 2;

        private void RemoveIndent() => _indentation -= 2;
    }
}
