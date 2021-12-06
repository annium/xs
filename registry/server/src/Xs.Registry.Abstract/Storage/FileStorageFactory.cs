using System.Collections.Generic;

namespace Xs.Registry.Abstract.Storage;

internal class FileStorageFactory : IStorageFactory
{
    private readonly IDictionary<string, IStorage> _storages = new Dictionary<string, IStorage>();

    public IStorage Create(string root)
    {
        lock(_storages)
        {
            if (_storages.ContainsKey(root))
                return _storages[root];

            return _storages[root] = new FileStorage(root);
        }
    }
}