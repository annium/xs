namespace Server.Abstractions.Storage;

public interface IStorageFactory
{
    IStorage Create(string root);
}