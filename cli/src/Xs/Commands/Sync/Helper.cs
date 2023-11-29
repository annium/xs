using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Linq;
using LibGit2Sharp;

namespace Xs.Commands.Sync;

internal static class Helper
{
    private static readonly IReadOnlyCollection<FileStatus> Statuses = Enum.GetValues<FileStatus>()
        .Except(FileStatus.Unaltered.Yield())
        .ToArray();

    private static readonly IReadOnlyDictionary<FileStatus, string> StatusLabels;

    static Helper()
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

    public static IReadOnlyCollection<SyncFileChange> GetProjectChanges(SyncProject project)
    {
        try
        {
            using var repo = new Repository(project.Path);

            var changes = repo.RetrieveStatus().Where(x => x.State is not FileStatus.Ignored).ToArray();
            if (changes.Length == 0)
                return Array.Empty<SyncFileChange>();

            var changeStates = new List<SyncFileChange>();
            foreach (var change in changes)
            {
                var description = change.State switch
                {
                    FileStatus.RenamedInIndex
                        => $"{change.HeadToIndexRenameDetails.OldFilePath} -> {change.HeadToIndexRenameDetails.NewFilePath}",
                    FileStatus.RenamedInWorkdir
                        => $"{change.IndexToWorkDirRenameDetails.OldFilePath} -> {change.IndexToWorkDirRenameDetails.NewFilePath}",
                    _ => change.FilePath
                };
                var status = Statuses
                    .Where(x => change.State.HasFlag(x))
                    .Select(x => StatusLabels[x])
                    .Distinct()
                    .Join(", ");
                changeStates.Add(new SyncFileChange(status, description));
            }

            return changeStates;
        }
        catch (Exception exception)
        {
            throw new SyncException(project.Path, exception);
        }
    }
}

internal sealed record SyncFileChange(string Status, string Description);

internal sealed class SyncException(string path, Exception exception) : Exception
{
    public string Path { get; } = path;
    public Exception Exception { get; } = exception;
}
