using System.Collections.Concurrent;
using Annium.Xs.Server.Abstractions.Services;

namespace Annium.Xs.Server.Abstractions.Internal.Services;

internal class FileStorageFactory : IStorageFactory
{
    private readonly ConcurrentDictionary<string, IStorage> _storages = new();

    public IStorage Create(string root) => _storages.GetOrAdd(root, path => new FileStorage(path));
}
