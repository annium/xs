namespace Annium.Xs.Server.Abstractions.Services;

public interface IStorageFactory
{
    IStorage Create(string root);
}
