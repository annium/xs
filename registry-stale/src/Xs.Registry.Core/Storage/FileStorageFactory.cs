using System.Collections.Generic;

namespace Xs.Registry.Core.Storage
{
    internal class FileStorageFactory : IStorageFactory
    {
        private IDictionary<string, IStorage> storages = new Dictionary<string, IStorage>();

        public IStorage Create(string root)
        {
            lock(storages)
            {
                if (storages.ContainsKey(root))
                    return storages[root];

                return storages[root] = new FileStorage(root);
            }
        }
    }
}