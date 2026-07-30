using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Xs.Server.Abstractions.Services;

namespace Annium.Xs.Server.Abstractions.Internal.Services;

internal class FileStorage : IStorage
{
    private const int CopyBufferSize = 81920;

    private readonly string _root;
    private readonly string _rootPrefix;

    public FileStorage(string root)
    {
        _root = Path.GetFullPath(root);
        _rootPrefix = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_root);
    }

    public Task<bool> ExistsAsync(string name)
    {
        var path = GetPath(name);
        var result = File.Exists(path);

        return Task.FromResult(result);
    }

    public Task<Stream> GetAsync(string name)
    {
        var path = GetPath(name);
        var content = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Task.FromResult<Stream>(content);
    }

    public async Task SaveAsync(string name, Stream stream)
    {
        var path = GetPath(name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var fs = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fs, CopyBufferSize);
    }

    public Task DeleteAsync(string name)
    {
        var path = GetPath(name);
        var dir = Path.GetDirectoryName(path)!;

        // deleting an absent artifact is a no-op. File.Delete alone doesn't give that guarantee -
        // it only tolerates a missing file while its directory still exists, and the cleanup below
        // removes emptied directories. A caller replaying a delete (e.g. a rollback restoring an
        // artifact) would otherwise hit DirectoryNotFoundException.
        if (!Directory.Exists(dir))
            return Task.CompletedTask;

        File.Delete(path);

        // recursively cleanup
        while (dir != _root && Directory.Exists(dir))
        {
            // if any files - no need to delete dir
            if (Directory.GetFileSystemEntries(dir).Length > 0)
                break;

            // current dir is not root and is empty - delete it
            Directory.Delete(dir);

            // go up
            dir = Path.GetDirectoryName(dir)!;
        }

        return Task.CompletedTask;
    }

    private string GetPath(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Artifact name is empty.", nameof(name));

        var path = Path.GetFullPath(Path.Combine(_root, name));

        // `name` is built from request-supplied package id/version, so a traversal segment would
        // otherwise resolve outside the storage root and turn Save/Get/Delete into arbitrary
        // file access. Compare against the root plus a separator so a sibling directory sharing
        // the root's prefix (e.g. "<root>-other") is not accepted either.
        if (!path.StartsWith(_rootPrefix, StringComparison.Ordinal))
            throw new ArgumentException($"Artifact name '{name}' resolves outside the storage root.", nameof(name));

        return path;
    }
}
