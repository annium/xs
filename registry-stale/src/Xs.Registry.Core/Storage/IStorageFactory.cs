namespace Xs.Registry.Core.Storage
{
    public interface IStorageFactory
    {
        IStorage Create(string root);
    }
}