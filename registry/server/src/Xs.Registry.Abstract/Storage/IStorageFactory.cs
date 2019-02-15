namespace Xs.Registry.Abstract.Storage
{
    public interface IStorageFactory
    {
        IStorage Create(string root);
    }
}