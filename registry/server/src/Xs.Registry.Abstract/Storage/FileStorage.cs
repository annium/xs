using System;
using System.IO;
using System.Threading.Tasks;

namespace Xs.Registry.Abstract.Storage;

internal class FileStorage : IStorage
{
    private const int CopyBufferSize = 81920;

    private readonly string _root;

    public FileStorage(
        string root
    )
    {
        _root = Path.GetFullPath(root);
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

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        await using (var fs = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fs, CopyBufferSize);
        }
    }

    public Task DeleteAsync(string name)
    {
        var path = GetPath(name);
        var dir = Path.GetDirectoryName(path);

        File.Delete(path);

        // recursively cleanup
        while (dir != _root)
        {
            // if any files - no need to delete dir
            if (Directory.GetFileSystemEntries(dir).Length > 0)
                break;

            // current dir is not root and is empty - delete it
            Directory.Delete(dir);

            // go up
            dir = Path.GetDirectoryName(dir);
        }

        return Task.CompletedTask;
    }

    private string GetPath(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"Given {name} is empty.");

        return Path.GetFullPath(Path.Combine(_root, name));
    }
}